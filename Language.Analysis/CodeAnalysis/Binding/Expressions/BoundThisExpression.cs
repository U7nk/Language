using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundThisExpression : BoundExpression
{
    public BoundThisExpression(TypeSymbol type)
    {
        Type = type;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ThisExpression;
    internal override TypeSymbol Type { get; }
}