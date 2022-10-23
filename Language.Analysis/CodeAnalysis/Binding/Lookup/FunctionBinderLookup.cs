using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class FunctionBinderLookup : BaseBinderLookup
{
    public TypeSymbol CurrentType { get; }
    public FunctionSymbol CurrentFunction { get; }

    public FunctionBinderLookup(
        TypeSymbol currentType,
        ImmutableArray<TypeSymbol> availableTypes,
        FunctionSymbol currentFunction) : base(availableTypes)
    {
        CurrentType = currentType;
        CurrentFunction = currentFunction;
    }
}