using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variables, BoundStatement statement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Variables = variables;
        Statement = statement;
    }

    public BoundStatement Statement { get; }

    public ImmutableArray<VariableSymbol> Variables { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
}