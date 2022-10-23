using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundFieldAccessExpression : BoundExpression
{
    public BoundFieldAccessExpression(FieldSymbol fieldSymbol)
    {
        FieldSymbol = fieldSymbol;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.FieldAccessExpression;
    internal override TypeSymbol Type => FieldSymbol.Type;
    public FieldSymbol FieldSymbol { get; }
}