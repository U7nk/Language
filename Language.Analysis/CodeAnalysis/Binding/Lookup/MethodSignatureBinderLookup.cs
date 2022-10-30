using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class MethodSignatureBinderLookup : BaseBinderLookup
{
    public MethodSignatureBinderLookup(ImmutableArray<TypeSymbol> availableTypes, TypeSymbol containingType)
        : base(availableTypes)
    {
        ContainingType = containingType;
    }
    public TypeSymbol ContainingType { get; }
}