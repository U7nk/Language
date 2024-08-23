using System;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundNamespaceExpression : BoundExpression
{
    public BoundNamespaceExpression(NameExpressionOrMemberAccessExpressionSyntax syntax, NamespaceSymbol namespaceSymbol)
        : base(syntax.IsNameExpression ? syntax.NameExpression : syntax.MemberAccess)
    {
        NamespaceSymbol = namespaceSymbol;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.NamespaceExpression;
    public NamespaceSymbol NamespaceSymbol { get; }
    internal override TypeSymbol Type => throw new NotImplementedException("REFACTOR!!! THIS SHOULD WORK OTHER WAY");
}