using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding;

internal class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
    {
        Type = type;
        Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    internal override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
}