using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class TypeBinderLookup : BinderLookup
{
    public TypeBinderLookup(TypeSymbol currentType, DeclarationsBag declarationsBag) : base(declarationsBag)
    {
        CurrentType = currentType;
    }

    public TypeSymbol CurrentType { get; }
}