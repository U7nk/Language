using System;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundUnaryExpression : BoundExpression
{
    internal UnaryExpressionSyntax? Syntax { get; }
    internal BoundUnaryOperator Op { get; }
    internal BoundExpression Operand { get; }
    internal override TypeSymbol Type => Operand.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;

    public BoundUnaryExpression(UnaryExpressionSyntax? syntax, BoundUnaryOperator op, BoundExpression operand) : base(syntax)
    {
        Syntax = syntax;
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
                return new BoundLiteralExpression(literal.Syntax, !value, TypeSymbol.Bool);
            }

            return new BoundUnaryExpression(null,
                BoundUnaryOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Bool) ?? throw new InvalidOperationException(),
                condition);
        }

        return null;
    }
}