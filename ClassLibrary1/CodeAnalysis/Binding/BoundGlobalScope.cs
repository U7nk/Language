using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope? Previous { get; }

    public BoundGlobalScope(BoundGlobalScope? previous,
        ImmutableArray<Diagnostic> diagnostics,
        FunctionSymbol? mainFunction,
        FunctionSymbol? scriptMainFunction,
        ImmutableArray<FunctionSymbol> functions,
        ImmutableArray<VariableSymbol> variables,
        BoundBlockStatement statement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptMainFunction = scriptMainFunction;
        Functions = functions;
        Variables = variables;
        Statement = statement;
    }

    public BoundBlockStatement Statement { get; }

    public ImmutableArray<VariableSymbol> Variables { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public FunctionSymbol? MainFunction { get; }
    public FunctionSymbol? ScriptMainFunction { get; }
    public ImmutableArray<FunctionSymbol> Functions { get; }
}