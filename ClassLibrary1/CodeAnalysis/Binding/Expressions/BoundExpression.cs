using System;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding;

public abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }
}