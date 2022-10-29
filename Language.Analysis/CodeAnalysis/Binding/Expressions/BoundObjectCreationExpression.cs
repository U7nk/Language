using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundObjectCreationExpression : BoundExpression
{
    public BoundObjectCreationExpression(SyntaxNode? syntax, TypeSymbol type) : base(syntax)
    {
        Type = type;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ObjectCreationExpression;
    internal override TypeSymbol Type { get; }
}