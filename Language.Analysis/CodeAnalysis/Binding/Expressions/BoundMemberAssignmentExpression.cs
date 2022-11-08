using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundMemberAssignmentExpression : BoundExpression
{
    public BoundExpression MemberAccess { get; }
    public BoundExpression RightValue { get; }

    public BoundMemberAssignmentExpression(SyntaxNode? syntax, BoundExpression memberAccess, BoundExpression rightValue) : base(syntax)
    {
        MemberAccess = memberAccess;
        RightValue = rightValue;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MemberAssignmentExpression;
    internal override TypeSymbol Type => MemberAccess.Type;
}