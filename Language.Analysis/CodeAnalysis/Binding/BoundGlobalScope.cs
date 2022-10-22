using System.Collections.Immutable;
using Language.CodeAnalysis.Symbols;

namespace Language.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope? previous,
        ImmutableArray<Diagnostic> diagnostics,
        FunctionSymbol? mainFunction,
        FunctionSymbol? scriptMainFunction,
        ImmutableArray<TypeSymbol> types,
        ImmutableArray<VariableSymbol> variables,
        BoundBlockStatement statement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptMainFunction = scriptMainFunction;
        Types = types;
        Variables = variables;
        Statement = statement;
    }

    public BoundGlobalScope? Previous { get; }
    public BoundBlockStatement Statement { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public FunctionSymbol? MainFunction { get; }
    public FunctionSymbol? ScriptMainFunction { get; }
    public ImmutableArray<TypeSymbol> Types { get; }
}