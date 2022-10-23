using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class TypeBinderLookup : BaseBinderLookup
{
    public TypeBinderLookup(TypeSymbol currentType, ImmutableArray<TypeSymbol> availableTypes) : base(availableTypes)
    {
        CurrentType = currentType;
    }

    public TypeSymbol CurrentType { get; }
}