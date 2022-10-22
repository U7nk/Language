using System;
using Language.CodeAnalysis.Symbols;

namespace Language.CodeAnalysis.Binding;

public abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }
}