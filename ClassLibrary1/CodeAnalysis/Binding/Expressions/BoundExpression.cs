using System;

namespace Wired.CodeAnalysis.Binding;

abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }
}