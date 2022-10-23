namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

    public ParenthesizedExpressionSyntax(
        SyntaxTree syntaxTree,
        SyntaxToken openParenthesisToken,
        ExpressionSyntax expression,
        SyntaxToken closeParenthesisToken) 
        : base(syntaxTree)
    {
        OpenParenthesisToken = openParenthesisToken;
        Expression = expression;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken CloseParenthesisToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken OpenParenthesisToken { get; }
    
}