using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundMemberAccessExpression : BoundExpression
{
    public BoundExpression Left { get; }
    public BoundExpression Member { get; }

    public BoundMemberAccessExpression(BoundExpression left, BoundExpression member)
    {
        Left = left;
        Member = member;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MemberAccessExpression;
    internal override TypeSymbol Type => Member.Type;
}