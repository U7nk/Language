using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundMemberAssignmentExpression : BoundExpression
{
    public BoundExpression MemberAccess { get; }
    public BoundExpression RightValue { get; }

    public BoundMemberAssignmentExpression(BoundExpression memberAccess, BoundExpression rightValue)
    {
        MemberAccess = memberAccess;
        RightValue = rightValue;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MemberAssignmentExpression;
    internal override TypeSymbol Type => MemberAccess.Type;
}