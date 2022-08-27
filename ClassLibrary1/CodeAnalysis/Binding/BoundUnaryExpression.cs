using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryExpression : BoundExpression
{
    internal BoundUnaryOperatorKind OperatorKind { get; }
    internal BoundExpression Operand { get; }
    internal override Type Type => this.Operand.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
    {
        this.OperatorKind = operatorKind;
        this.Operand = operand;
    }
}