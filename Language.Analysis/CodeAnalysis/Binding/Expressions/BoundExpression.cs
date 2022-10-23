using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }
}