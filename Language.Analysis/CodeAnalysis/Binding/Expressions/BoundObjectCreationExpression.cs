using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundObjectCreationExpression : BoundExpression
{
    public BoundObjectCreationExpression(TypeSymbol type)
    {
        Type = type;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ObjectCreationExpression;
    internal override TypeSymbol Type { get; }
}