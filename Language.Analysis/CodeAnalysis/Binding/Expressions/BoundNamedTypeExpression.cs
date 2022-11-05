using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundNamedTypeExpression : BoundExpression
{
    public BoundNamedTypeExpression(NameExpressionSyntax nameExpression, TypeSymbol symbol) : base(nameExpression)
    {
        Type = symbol;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.NamedTypeExpression;
    internal override TypeSymbol Type { get; }
}