using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundMemberAccessExpression : BoundExpression
{
    public BoundExpression Left { get; }
    public BoundExpression Member { get; }

    public BoundMemberAccessExpression(Option<SyntaxNode> syntax, BoundExpression left, BoundExpression member) : base(syntax)
    {
        Left = left;
        Member = member;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MemberAccessExpression;
    internal override TypeSymbol Type => Member.Type;
}