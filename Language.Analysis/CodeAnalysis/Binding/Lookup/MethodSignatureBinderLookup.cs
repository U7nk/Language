using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class MethodSignatureBinderLookup : BinderLookup
{
    public MethodSignatureBinderLookup(ImmutableArray<TypeSymbol> availableTypes, TypeSymbol containingType,
                                       bool isTopMethod, DeclarationsBag declarationsBag)
        : base(availableTypes, declarationsBag)
    {
        ContainingType = containingType;
        IsTopMethod = isTopMethod;
    }
    public TypeSymbol ContainingType { get; }
    public bool IsTopMethod { get; }
}