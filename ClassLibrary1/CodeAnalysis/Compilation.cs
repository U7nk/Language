using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Lowering;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

public sealed class Compilation
{
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public bool IsScript { get; }
    public Compilation? Previous { get; }
    public FunctionSymbol? MainFunction => GlobalScope.MainFunction;

    BoundGlobalScope? _globalScope;

    Compilation(bool isScript, Compilation? previous, SyntaxTree[] syntaxTree)
    {
        SyntaxTrees = syntaxTree.ToImmutableArray();
        IsScript = isScript;
        Previous = previous;
    }

    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: false, null, syntaxTrees);
    }
    
    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: true, previous, syntaxTrees);
    }
    
    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope is null)
            {
                var boundGlobalScope = Binder
                    .BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref _globalScope, boundGlobalScope, null);
            }

            return _globalScope;
        }
    }

    BoundProgram GetProgram()
    {
        var previous = Previous?.GetProgram();
        return Binder.BindProgram(IsScript, previous, GlobalScope);
    }
    
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics, null);

        var program = GetProgram();
        if (program.Diagnostics.Any())
            return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

        var statement = GetStatement();
        
        // var cfgPath = "control_flow.dot";
        // var cfg = ControlFlowGraph.Create(
        //     !GlobalScope.Statement.Statements.Any() && GlobalScope.Functions.Any()
        //         ? program.FunctionBodies.Last().Value
        //         : statement);        // using (var streamWriter = new StreamWriter(cfgPath)) 
        //     cfg.WriteTo(streamWriter);
        
        var evaluator = new Evaluator(program, variables);
        var result = evaluator.Evaluate();
        return new(ImmutableArray<Diagnostic>.Empty, result);
    }

    public void EmitTree(TextWriter writer)
    {
        if (GlobalScope.MainFunction is not null)
            EmitTree(GlobalScope.MainFunction, writer);
        else if (GlobalScope.ScriptMainFunction is not null) 
            EmitTree(GlobalScope.ScriptMainFunction, writer);

        foreach (var function in GlobalScope.Functions
                     .Where(x => !Equals(x, GlobalScope.MainFunction)
                                 && !Equals(x, GlobalScope.ScriptMainFunction)))
        {
            EmitTree(function, writer);
        }
    }
    
    public void EmitTree(FunctionSymbol function, TextWriter writer)
    {
        var program = GetProgram();
        function.WriteTo(writer);
        writer.WriteLine();
        if (!program.Functions.TryGetValue(function, out var body))
            return;
        
        body.WriteTo(writer);
    }

    BoundBlockStatement GetStatement()
    {
        var result = GlobalScope.Statement;
        return Lowerer.Lower(result);
    }
}