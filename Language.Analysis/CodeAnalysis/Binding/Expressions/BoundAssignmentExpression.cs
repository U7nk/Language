using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundAssignmentExpression : BoundExpression
{
    public BoundExpression Left { get; }
    public BoundExpression Initializer { get; }

    public BoundAssignmentExpression(Option<SyntaxNode> syntax, BoundExpression left, BoundExpression initializer) : base(syntax)
    {
        Left = left;
        Initializer = initializer;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    internal override TypeSymbol Type => Left.Type;
}