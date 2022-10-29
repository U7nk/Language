using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundThisExpression : BoundExpression
{
    public BoundThisExpression(SyntaxNode syntax,TypeSymbol type) : base(syntax)
    {
        Type = type;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.ThisExpression;
    internal override TypeSymbol Type { get; }
}