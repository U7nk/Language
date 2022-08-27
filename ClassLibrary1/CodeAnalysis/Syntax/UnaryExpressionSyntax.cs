using System.Collections.Generic;

namespace Wired.CodeAnalysis;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand)
    {
        this.OperatorToken = operatorToken;
        this.Operand = operand;
    }

    public ExpressionSyntax Operand { get; }
    public SyntaxToken OperatorToken { get; }
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return this.OperatorToken;
        yield return this.Operand;
    }
}