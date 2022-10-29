using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }

    public BoundExpression(SyntaxNode? syntax) : base(syntax)
    {
    }
}