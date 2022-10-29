using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundFieldExpression : BoundExpression
{
    public BoundFieldExpression(SyntaxNode syntax, FieldSymbol fieldSymbol) : base(syntax)
    {
        FieldSymbol = fieldSymbol;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.FieldExpression;
    internal override TypeSymbol Type => FieldSymbol.Type;
    public FieldSymbol FieldSymbol { get; }
}