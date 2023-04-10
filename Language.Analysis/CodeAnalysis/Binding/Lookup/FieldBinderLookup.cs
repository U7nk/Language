using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class FieldBinderLookup : BinderLookup
{
    public FieldBinderLookup(TypeSymbol containingType, DeclarationsBag declarationsBag) : base(declarationsBag)
    {
        ContainingType = containingType;
    }
    
    public TypeSymbol ContainingType { get; }
}