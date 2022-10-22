using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding;

public class BoundFieldExpression : BoundExpression
{
    public FieldSymbol Field { get; }

    public BoundFieldExpression(FieldSymbol field)
    {
        Field = field;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.FieldExpression;
    internal override TypeSymbol Type => Field.Type;
}