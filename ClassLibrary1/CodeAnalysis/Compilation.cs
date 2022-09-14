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
    public SyntaxTree SyntaxTree { get; }
    public Compilation? Previous { get; }

    public Compilation(SyntaxTree syntaxTree) 
        : this(null, syntaxTree)
    { }

    BoundGlobalScope? _globalScope;

    Compilation(Compilation? previous, SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
        Previous = previous;
    }

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope is null)
            {
                var boundGlobalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                Interlocked.CompareExchange(ref _globalScope, boundGlobalScope, null);
            }

            return _globalScope;
        }
    }
    
    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return new Compilation(this, syntaxTree);
    }
    
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
    {
        var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics)
            .ToImmutableArray();
        if (diagnostics.Any())
            return new(diagnostics, null);

        var program = Binder.BindProgram(GlobalScope);
        if (program.Diagnostics.Any())
            return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

        var statement = GetStatement();
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