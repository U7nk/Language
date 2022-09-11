namespace Wired.CodeAnalysis.Binding;

internal class BoundErrorExpression : BoundExpression
{
    
    internal override TypeSymbol Type => TypeSymbol.Error;
    internal override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}