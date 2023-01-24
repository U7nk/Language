using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundBinaryExpression : BoundExpression
{
    internal BoundBinaryOperator Op { get; }
    internal BoundExpression Left { get; }
    internal BoundExpression Right { get; }
    internal override TypeSymbol Type => Op.ResultType;
    internal override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public BoundBinaryExpression(Option<SyntaxNode> syntax, BoundExpression left, BoundBinaryOperator op, BoundExpression right) : base(syntax)
    {
        Left = left;
        Op = op;
        Right = right;
    }
}