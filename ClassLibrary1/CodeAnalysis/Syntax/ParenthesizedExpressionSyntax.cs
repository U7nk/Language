using System.Collections.Generic;

namespace Wired.CodeAnalysis;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

    public ParenthesizedExpressionSyntax(
        SyntaxToken openParenthesisToken,
        ExpressionSyntax expression,
        SyntaxToken closeParenthesisToken)
    {
        this.OpenParenthesisToken = openParenthesisToken;
        this.Expression = expression;
        this.CloseParenthesisToken = closeParenthesisToken;
    }

    public SyntaxToken CloseParenthesisToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken OpenParenthesisToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return this.OpenParenthesisToken;
        yield return this.Expression;
        yield return this.CloseParenthesisToken;
    }
}