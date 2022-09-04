using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    {
        this.Left = left;
        this.OperatorToken = operatorToken;
        this.Right = right;
    }

    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
}