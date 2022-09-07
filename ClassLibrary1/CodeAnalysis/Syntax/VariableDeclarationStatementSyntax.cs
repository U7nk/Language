namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationStatementSyntax : StatementSyntax
{
    public SyntaxToken KeywordToken { get; }
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax InitializerExpression { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
    
    public VariableDeclarationStatementSyntax(
        SyntaxToken keywordToken,
        SyntaxToken identifierToken,
        SyntaxToken equalsToken,
        ExpressionSyntax initializerExpression,
        SyntaxToken semicolonToken)
    {
        this.KeywordToken = keywordToken;
        this.IdentifierToken = identifierToken;
        this.EqualsToken = equalsToken;
        this.InitializerExpression = initializerExpression;
        this.SemicolonToken = semicolonToken;
    }
}