using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class MethodSignatureBinderLookup : BaseBinderLookup
{
    public MethodSignatureBinderLookup(ImmutableArray<TypeSymbol> availableTypes, TypeSymbol containingType,
                                       bool isTopMethod)
        : base(availableTypes)
    {
        ContainingType = containingType;
        IsTopMethod = isTopMethod;
    }
    public TypeSymbol ContainingType { get; }
    public bool IsTopMethod { get; }
}