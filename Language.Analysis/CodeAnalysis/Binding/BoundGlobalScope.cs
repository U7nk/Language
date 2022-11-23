using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
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
        DeclarationsBag declarationsBag)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainMethod = mainMethod;
        ScriptMainMethod = scriptMainMethod;
        Types = types;
        Variables = variables;
        DeclarationsBag = declarationsBag;
    }

    public BoundGlobalScope? Previous { get; }
    public DeclarationsBag DeclarationsBag { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public MethodSymbol? MainMethod { get; }
    public MethodSymbol? ScriptMainMethod { get; }
    public ImmutableArray<TypeSymbol> Types { get; }
}