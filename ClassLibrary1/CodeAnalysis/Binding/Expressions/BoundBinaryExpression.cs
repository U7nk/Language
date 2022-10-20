using System;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryExpression : BoundExpression
{
    internal BoundBinaryOperator Op { get; }
    internal BoundExpression Left { get; }
    internal BoundExpression Right { get; }
    internal override TypeSymbol Type => Op.ResultType;
    internal override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Left = left;
        Op = op;
        Right = right;
    }
}