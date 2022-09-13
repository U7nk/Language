namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationSyntax : SyntaxNode
{
    public VariableDeclarationSyntax(SyntaxToken keywordToken, SyntaxToken identifierToken)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationSyntax;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    
}