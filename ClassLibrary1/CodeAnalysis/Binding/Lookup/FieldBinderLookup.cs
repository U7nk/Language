using System.Collections.Immutable;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding.Lookup;

public class FieldBinderLookup : BaseBinderLookup
{
    public FieldBinderLookup(ImmutableArray<TypeSymbol> availableTypes) : base(availableTypes)
    {
    }
}