using System;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundUnaryExpression : BoundExpression
{
    internal BoundUnaryOperator Op { get; }
    internal BoundExpression Operand { get; }
    internal override TypeSymbol Type => Operand.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;

    public BoundUnaryExpression(SyntaxNode? syntax, BoundUnaryOperator op, BoundExpression operand) : base(syntax)
    {
        Op = op;
        Operand = operand;
    }

    public static BoundExpression? Negate(BoundExpression condition)
    {
        if (Equals(condition.Type, BuiltInTypeSymbols.Bool))
        {
            if (condition is BoundLiteralExpression literal)
            {
                var value = (bool)(literal.Value ?? throw new InvalidOperationException());
                return new BoundLiteralExpression(literal.Syntax, !value, BuiltInTypeSymbols.Bool);
            }

            return new BoundUnaryExpression(null,
                BoundUnaryOperator.Bind(SyntaxKind.BangToken, BuiltInTypeSymbols.Bool) ?? throw new InvalidOperationException(),
                condition);
        }

        return null;
    }
}