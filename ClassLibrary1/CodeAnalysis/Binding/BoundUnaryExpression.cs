using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryExpression : BoundExpression
{
    internal BoundUnaryOperator Op { get; }
    internal BoundExpression Operand { get; }
    internal override Type Type => this.Operand.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
    {
        this.Op = op;
        this.Operand = operand;
    }
}