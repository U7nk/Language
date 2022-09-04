using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

public sealed class Compilation
{
    public SyntaxTree SyntaxTree { get; }

    public Compilation(SyntaxTree syntaxTree)
    {
        this.SyntaxTree = syntaxTree;
    }
    
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
    {
        var binder = new Binder(variables);
        var boundExpression = binder.BindExpression(this.SyntaxTree.Root);
        var diagnostics = binder.Diagnostics.Concat(this.SyntaxTree.Diagnostics)
            .ToImmutableArray();
        if (diagnostics.Any())
        {
            return new EvaluationResult(diagnostics, null);
        }
        var evaluator = new Evaluator(boundExpression, variables);
        var result = evaluator.Evaluate();
        return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, result);
    }
}