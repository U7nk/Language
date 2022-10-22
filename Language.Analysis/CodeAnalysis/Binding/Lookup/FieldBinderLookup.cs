using System.Collections.Immutable;
using Language.CodeAnalysis.Symbols;

namespace Language.CodeAnalysis.Binding.Lookup;

public class FieldBinderLookup : BaseBinderLookup
{
    public FieldBinderLookup(ImmutableArray<TypeSymbol> availableTypes) : base(availableTypes)
    {
    }
}