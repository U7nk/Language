using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundFieldAssignmentExpression : BoundExpression
{
    public BoundExpression ObjectAccess { get; }
    public FieldSymbol Field { get; }
    public BoundExpression Initializer { get; }

    public BoundFieldAssignmentExpression(BoundExpression objectAccess, FieldSymbol field, BoundExpression initializer)
    {
        ObjectAccess = objectAccess;
        Field = field;
        Initializer = initializer;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.FieldAssignmentExpression;
    internal override TypeSymbol Type => Field.Type;
}