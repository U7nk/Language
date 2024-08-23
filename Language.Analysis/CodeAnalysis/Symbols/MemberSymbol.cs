using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public abstract class MemberSymbol(Option<SyntaxNode> declarationSyntax, string name, TypeSymbol containingType, TypeSymbol type) 
    : Symbol(declarationSyntax, name), ITypedSymbol, ITypeMemberSymbol
{
    public TypeSymbol Type { get; } = type;
    public Option<TypeSymbol> ContainingType { get; } = containingType;
}