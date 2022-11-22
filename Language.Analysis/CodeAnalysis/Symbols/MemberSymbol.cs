using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public abstract class MemberSymbol : Symbol, ITypedSymbol
{
    protected MemberSymbol(Option<SyntaxNode> declarationSyntax, string name,TypeSymbol? containingType, TypeSymbol type) 
        : base(declarationSyntax, name, containingType)
    {
        Type = type;
    }
    public TypeSymbol Type { get; }
}