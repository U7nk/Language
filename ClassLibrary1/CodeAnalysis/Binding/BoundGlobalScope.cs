using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variables, BoundExpression expression)
    {
        this.Previous = previous;
        this.Diagnostics = diagnostics;
        this.Variables = variables;
        this.Expression = expression;
    }

    public BoundExpression Expression { get; }

    public ImmutableArray<VariableSymbol> Variables { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}