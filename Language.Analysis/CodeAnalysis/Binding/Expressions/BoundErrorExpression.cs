using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;


internal class BoundErrorExpression : BoundExpression
{
    public BoundErrorExpression() : base(null)
    {
        
    }
    internal override TypeSymbol Type => TypeSymbol.BuiltIn.Error();
    internal override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}