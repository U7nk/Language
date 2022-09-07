namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationSyntax : SyntaxNode
{
    public VariableDeclarationSyntax(SyntaxToken keywordToken, SyntaxToken identifierToken)
    {
        this.KeywordToken = keywordToken;
        this.IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationSyntax;
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    
}