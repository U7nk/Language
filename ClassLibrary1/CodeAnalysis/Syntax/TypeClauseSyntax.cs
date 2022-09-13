namespace Wired.CodeAnalysis.Syntax;

public class TypeClauseSyntax : SyntaxNode
{
    public TypeClauseSyntax(SyntaxToken colon, SyntaxToken identifier)
    {
        Colon = colon;
        Identifier = identifier;
    }

    public SyntaxToken Colon { get; }
    public SyntaxToken Identifier { get; }
    public override SyntaxKind Kind => SyntaxKind.TypeClause;
}