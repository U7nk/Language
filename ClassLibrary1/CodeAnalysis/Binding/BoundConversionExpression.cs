namespace Wired.CodeAnalysis.Binding;

internal class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
    {
        this.Type = type;
        this.Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    internal override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
}