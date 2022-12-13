using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(Option<SyntaxNode> syntax, TypeSymbol type, BoundExpression expression) : base(syntax)
    {
        Type = type;
        Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    internal override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
}