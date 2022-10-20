using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding.Lookup;

public class BaseBinderLookup
{
    public BaseBinderLookup(ImmutableArray<TypeSymbol> availableTypes)
    {
        AvailableTypes = availableTypes;
    }
    
    public ImmutableArray<TypeSymbol> AvailableTypes { get; }
    
    public TypeSymbol? LookupType(string name)
    {
        return AvailableTypes.SingleOrDefault(t => t.Name == name);
    }
}