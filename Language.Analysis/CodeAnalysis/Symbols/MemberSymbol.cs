using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public abstract class MemberSymbol : Symbol
{
    protected MemberSymbol(ImmutableArray<SyntaxNode> declarationSyntax, string name, TypeSymbol type) : base(declarationSyntax, name)
    {
        Type = type;
    }
    public TypeSymbol Type { get; }
}