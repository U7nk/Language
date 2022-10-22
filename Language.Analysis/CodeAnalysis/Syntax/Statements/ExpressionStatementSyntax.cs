namespace Language.CodeAnalysis.Syntax;

public class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken semicolonToken) 
        : base(syntaxTree)
    {
        Expression = expression;
        SemicolonToken = semicolonToken;
    }
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    public ExpressionSyntax Expression { get; }
    public SyntaxToken SemicolonToken { get; }

}