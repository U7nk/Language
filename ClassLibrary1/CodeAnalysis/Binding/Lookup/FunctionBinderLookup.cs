using System.Collections.Immutable;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding.Lookup;

public class FunctionBinderLookup
{
    public TypeSymbol CurrentType { get; }
    public FunctionSymbol CurrentFunction { get; }
    public ImmutableArray<TypeSymbol> AvailableTypes { get; }
    

    public FunctionBinderLookup(TypeSymbol currentType, ImmutableArray<TypeSymbol> availableTypes, FunctionSymbol currentFunction)
    {
        CurrentType = currentType;
        AvailableTypes = availableTypes;
        CurrentFunction = currentFunction;
    }
}