using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope? previous,
        ImmutableArray<Diagnostic> diagnostics,
        MethodSymbol? mainMethod,
        MethodSymbol? scriptMainMethod,
        ImmutableArray<TypeSymbol> types,
        ImmutableArray<VariableSymbol> variables,
        BoundBlockStatement statement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainMethod = mainMethod;
        ScriptMainMethod = scriptMainMethod;
        Types = types;
        Variables = variables;
        Statement = statement;
    }

    public BoundGlobalScope? Previous { get; }
    public BoundBlockStatement Statement { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public MethodSymbol? MainMethod { get; }
    public MethodSymbol? ScriptMainMethod { get; }
    public ImmutableArray<TypeSymbol> Types { get; }
}