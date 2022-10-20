using System;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryExpression : BoundExpression
{
    internal BoundUnaryOperator Op { get; }
    internal BoundExpression Operand { get; }
    internal override TypeSymbol Type => Operand.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;

    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
    {
        Op = op;
        Operand = operand;
    }

    public static BoundExpression? Negate(BoundExpression condition)
    {
        if (Equals(condition.Type, TypeSymbol.Bool))
        {
            if (condition is BoundLiteralExpression literal)
            {
                var value = (bool)(literal.Value ?? throw new InvalidOperationException());
                return new BoundLiteralExpression(!value, TypeSymbol.Bool);
            }

            return new BoundUnaryExpression(
                BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool) ?? throw new InvalidOperationException(),
                condition);
        }

        return null;
    }
}