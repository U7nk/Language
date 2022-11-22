using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class FieldBinderLookup : BinderLookup
{
    public FieldBinderLookup(ImmutableArray<TypeSymbol> availableTypes, TypeSymbol containingType, DeclarationsBag declarationsBag) : base(availableTypes, declarationsBag)
    {
        ContainingType = containingType;
    }
    
    public TypeSymbol ContainingType { get; }
}