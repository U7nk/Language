using System.Collections.Generic;

namespace Wired.CodeAnalysis;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    {
        this.Left = left;
        this.OperatorToken = operatorToken;
        this.Right = right;
    }

    public ExpressionSyntax Right { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Left { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return this.Left;
        yield return this.OperatorToken;
        yield return this.Right;
        
    }
}