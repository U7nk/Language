using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundMemberAssignmentExpression : BoundExpression
{
    public BoundMemberAccessExpression MemberAccess { get; }
    public BoundExpression RightValue { get; }

    public BoundMemberAssignmentExpression(BoundMemberAccessExpression memberAccess, BoundExpression rightValue)
    {
        MemberAccess = memberAccess;
        RightValue = rightValue;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MemberAssignmentExpression;
    internal override TypeSymbol Type => MemberAccess.Type;
}