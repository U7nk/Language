using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryExpression : BoundExpression
{
    internal BoundBinaryOperator Op { get; }
    internal BoundExpression Left { get; }
    internal BoundExpression Right { get; }
    internal override Type Type => this.Left.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        this.Left = left;
        this.Op = op;
        this.Right = right;
    }
}