using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Binding.Binders;
using Language.Analysis.CodeAnalysis.Binding.Binders.Class;
using Language.Analysis.CodeAnalysis.Binding.Binders.Namespace;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(ImmutableArray<Diagnostic> diagnostics,
                            Option<MethodSymbol> mainMethod,
                            ImmutableArray<TypeSymbol> types,
                            ImmutableArray<VariableSymbol> variables,
                            DeclarationsBag declarationsBag,
                            ImmutableArray<NamespaceBinder> typeBinders)
    {
        Diagnostics = diagnostics;
        MainMethod = mainMethod;
        Types = types;
        Variables = variables;
        DeclarationsBag = declarationsBag;
        TypeBinders = typeBinders;
    }

    public DeclarationsBag DeclarationsBag { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public Option<MethodSymbol> MainMethod { get; }
    public ImmutableArray<TypeSymbol> Types { get; }
    
    internal ImmutableArray<NamespaceBinder> TypeBinders { get; }
}