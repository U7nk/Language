using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

public sealed class Compilation
{
    public SyntaxTree SyntaxTree { get; }
    public Compilation? Previous { get; }

    public Compilation(SyntaxTree syntaxTree) 
        : this(null, syntaxTree)
    { }

    private BoundGlobalScope? globalScope;

    private Compilation(Compilation? previous, SyntaxTree syntaxTree)
    {
        this.SyntaxTree = syntaxTree;
        this.Previous = previous;
    }

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (this.globalScope is null)
            {
                var boundGlobalScope = Binder.BindGlobalScope(this.Previous?.GlobalScope, this.SyntaxTree.Root);
                Interlocked.CompareExchange(ref this.globalScope, boundGlobalScope, null);
            }

            return this.globalScope;
        }
    }
    
    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return new Compilation(this, syntaxTree);
    }
    
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
    {
        var diagnostics = this.SyntaxTree.Diagnostics.Concat(this.GlobalScope.Diagnostics)
            .ToImmutableArray();
        if (diagnostics.Any())
        {
            return new EvaluationResult(diagnostics, null);
        }

        var boundExpression = this.GlobalScope.Expression;
        var evaluator = new Evaluator(boundExpression, variables);
        var result = evaluator.Evaluate();
        return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, result);
    }
}