using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class MethodBinderLookup : BaseBinderLookup
{
    public TypeSymbol CurrentType { get; }
    public MethodSymbol CurrentMethod { get; }

    public MethodBinderLookup(
        TypeSymbol currentType,
        ImmutableArray<TypeSymbol> availableTypes,
        MethodSymbol currentMethod) : base(availableTypes)
    {
        CurrentType = currentType;
        CurrentMethod = currentMethod;
    }
}