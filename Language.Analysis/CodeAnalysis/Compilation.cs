using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Binders;
using Language.Analysis.CodeAnalysis.Emit;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

public sealed class Compilation
{
    Compilation(bool isScript, Compilation? previous, SyntaxTree[] syntaxTree)
    {
        SyntaxTrees = syntaxTree.ToImmutableArray();
        IsScript = isScript;
        Previous = previous;
    }

    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public bool IsScript { get; }
    public Compilation? Previous { get; }

    BoundGlobalScope? _globalScope;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope is not null)
                return _globalScope;

            var programBinder = new ProgramBinder(IsScript, Previous?._globalScope, SyntaxTrees);
            var boundGlobalScope = programBinder.BindGlobalScope();
            Interlocked.CompareExchange(ref _globalScope, boundGlobalScope, null);

            return _globalScope;
        }
    }

    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: false, null, syntaxTrees);
    }

    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: true, previous, syntaxTrees);
    }

    BoundProgram GetProgram()
    {
        var previous = Previous?.GetProgram();
        return ProgramBinder.BindProgram(IsScript, previous, GlobalScope);
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

        // var cfgPath = "control_flow.dot";
        // var cfg = ControlFlowGraph.Create(
        //     !GlobalScope.Statement.Statements.Any() && GlobalScope.Functions.Any()
        //         ? program.FunctionBodies.Last().Value
        //         : Lowerer.Lower(GlobalScope.Statement));        // using (var streamWriter = new StreamWriter(cfgPath)) 
        //     cfg.WriteTo(streamWriter);

        var evaluator = new Evaluator(program, variables);
        var result = evaluator.Evaluate();
        return new(ImmutableArray<Diagnostic>.Empty, result);
    }

    public void EmitTree(IndentedTextWriter writer)
    {
        if (GlobalScope.MainFunction is not null)
            EmitTree(GlobalScope.MainFunction, writer);
        else if (GlobalScope.ScriptMainFunction is not null)
            EmitTree(GlobalScope.ScriptMainFunction, writer);

        foreach (var type in GlobalScope.Types)
        {
            EmitTree(type, writer);
        }
    }

    public void EmitTree(TypeSymbol type, IndentedTextWriter writer)
    {
        type.WriteTo(writer);
        writer.WriteLine();
        writer.Write("{");
        foreach (var methodEntry in type.MethodTable)
        {
            writer.WriteLine();
            writer.Indent++;
            methodEntry.Key.WriteTo(writer);
            methodEntry.Value?.WriteTo(writer);
            writer.Indent--;
        }
        writer.Write("}");
    }
    
    public void EmitTree(FunctionSymbol function, TextWriter writer)
    {
        var program = GetProgram();
        function.WriteTo(writer);
        writer.WriteLine();
        var type = program.Types.SingleOrDefault(x => x.MethodTable.ContainsKey(function));
        if (type is null)
        {
            return;
        }
        
        type.MethodTable[function].Unwrap().WriteTo(writer);
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, 
        string[] refernces, string outputPaths)
    {
        var program = GetProgram();
        return new Emitter().Emit(program, moduleName, refernces, outputPaths);
    }
}