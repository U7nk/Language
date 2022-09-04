using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand)
    {
        this.OperatorToken = operatorToken;
        this.Operand = operand;
    }

    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Operand { get; }
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
}