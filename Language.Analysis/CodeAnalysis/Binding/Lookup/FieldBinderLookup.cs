using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class FieldBinderLookup : BaseBinderLookup
{
    public FieldBinderLookup(ImmutableArray<TypeSymbol> availableTypes) : base(availableTypes)
    {
    }
}