using System.Collections.Immutable;
using Language.CodeAnalysis.Symbols;

namespace Language.CodeAnalysis.Binding.Lookup;

public class TypeBinderLookup : BaseBinderLookup
{
    public TypeBinderLookup(TypeSymbol currentType, ImmutableArray<TypeSymbol> availableTypes) : base(availableTypes)
    {
        CurrentType = currentType;
    }

    public TypeSymbol CurrentType { get; }
}