namespace Wired.CodeAnalysis.Syntax;

public class TypeClauseSyntax : SyntaxNode
{
    public TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colon, SyntaxToken identifier) : base(syntaxTree)
    {
        Colon = colon;
        Identifier = identifier;
    }

    public SyntaxToken Colon { get; }
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}