using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

[OneOf(typeof(NameExpressionSyntax), "NameExpression", typeof(MemberAccessExpressionSyntax), "MemberAccess")]
partial class NameExpressionOrMemberAccessExpressionSyntax;


class BoundNamedTypeExpression : BoundExpression
{
    public BoundNamedTypeExpression(NameExpressionOrMemberAccessExpressionSyntax syntax, TypeSymbol symbol) 
        : base(syntax.IsNameExpression ? syntax.NameExpression : syntax.MemberAccess )
    {
        Type = symbol;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.NamedTypeExpression;
    internal override TypeSymbol Type { get; }
}