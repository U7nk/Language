using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryExpression : BoundExpression
{
    internal BoundBinaryOperatorKind OperatorKind { get; }
    internal BoundExpression Left { get; }
    internal BoundExpression Right { get; }
    internal override Type Type => this.Left.Type;
    internal override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
    {
        this.Left = left;
        this.OperatorKind = operatorKind;
        this.Right = right;
    }
}