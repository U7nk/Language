namespace Wired.CodeAnalysis.Syntax;

public class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionStatementSyntax(ExpressionSyntax expression, SyntaxToken semicolonToken)
    {
        this.Expression = expression;
        this.SemicolonToken = semicolonToken;
    }
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    public ExpressionSyntax Expression { get; }
    public SyntaxToken SemicolonToken { get; }

}