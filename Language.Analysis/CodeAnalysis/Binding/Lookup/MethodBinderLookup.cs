using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;

public class MethodBinderLookup : BinderLookup
{
    public TypeSymbol CurrentType { get; }
    public MethodSymbol CurrentMethod { get; }

    public MethodBinderLookup(DeclarationsBag declarationsBag,
        TypeSymbol currentType, MethodSymbol currentMethod) : base(declarationsBag)
    {
        CurrentType = currentType;
        CurrentMethod = currentMethod;
    }
}