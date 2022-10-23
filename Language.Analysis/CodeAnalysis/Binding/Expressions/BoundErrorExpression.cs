using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;


internal class BoundErrorExpression : BoundExpression
{
    public BoundErrorExpression()
    {
        
    }
    internal override TypeSymbol Type => TypeSymbol.Error;
    internal override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}