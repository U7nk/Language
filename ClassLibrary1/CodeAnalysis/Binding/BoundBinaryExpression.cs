using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryExpression : BoundExpression
{
    internal BoundBinaryOperator Op { get; }
    internal BoundExpression Left { get; }
    internal BoundExpression Right { get; }
    internal override Type Type => this.Op.ResultType;
    internal override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        this.Left = left;
        this.Op = op;
        this.Right = right;
    }
}