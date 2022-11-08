using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class BinderLookup
{
    public BinderLookup(ImmutableArray<TypeSymbol> availableTypes)
    {
        AvailableTypes = availableTypes;
    }
    
    public ImmutableArray<TypeSymbol> AvailableTypes { get; }
    
    public TypeSymbol? LookupType(string name)
    {
        return AvailableTypes.SingleOrDefault(t => t.Name == name);
    }

    public static ImmutableArray<Symbol> LookupSymbols(string name, BoundScope scope, TypeSymbol type)
    {
        var symbols = new List<Symbol>();
        var methods = type.LookupMethod(name);
        symbols.AddRange(methods);

        var field = type.LookupField(name);
        if (field is { })
            symbols.Add(field);
        
        scope.TryLookupVariable(name, out var variable);
        if (variable != null) 
            symbols.Add(variable);

        scope.TryLookupType(name, out var scopeType);
        if (scopeType != null)
            symbols.Add(scopeType);
        
        return symbols.ToImmutableArray();
    }
}