using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }

    public BoundGlobalScope(BoundGlobalScope? previous, 
        ImmutableArray<Diagnostic> diagnostics,
        ImmutableArray<FunctionSymbol> functions, 
        ImmutableArray<VariableSymbol> variables,
        BoundStatement statement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Functions = functions;
        Variables = variables;
        Statement = statement;
    }

    public BoundStatement Statement { get; }

    public ImmutableArray<VariableSymbol> Variables { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableArray<FunctionSymbol> Functions { get; }
}