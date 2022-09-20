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
    public Compilation? Previous { get; }

    public Compilation(params SyntaxTree[] syntaxTree) 
        : this(null, syntaxTree)
    { }

    BoundGlobalScope? _globalScope;

    Compilation(Compilation? previous, SyntaxTree[] syntaxTree)
    {
        SyntaxTrees = syntaxTree.ToImmutableArray();
        Previous = previous;
    }

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope is null)
            {
                var boundGlobalScope = Binder
                    .BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref _globalScope, boundGlobalScope, null);
            }

            return _globalScope;
        }
    }
    
    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return new Compilation(this, new []{syntaxTree});
    }
    
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics, null);
        
        var program = Binder.BindProgram(GlobalScope);
        if (program.Diagnostics.Any())
            return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

        var statement = GetStatement();
        
        var cfgPath = "control_flow.dot";
        var cfg = ControlFlowGraph.Create(
            !program.GlobalScope.Statement.Statements.Any() && program.GlobalScope.Functions.Any()
                ? program.FunctionBodies.Last().Value
                : statement);
        
        using (var streamWriter = new StreamWriter(cfgPath)) 
            cfg.WriteTo(streamWriter);
        
        var evaluator = new Evaluator(program.FunctionBodies, statement, variables);
        var result = evaluator.Evaluate();
        return new(ImmutableArray<Diagnostic>.Empty, result);
    }

    public void EmitTree(TextWriter writer)
    {
        var statement = GetStatement();
        statement.WriteTo(writer);
    }

    BoundBlockStatement GetStatement()
    {
        var result = GlobalScope.Statement;
        return Lowerer.Lower(result);
    }
}