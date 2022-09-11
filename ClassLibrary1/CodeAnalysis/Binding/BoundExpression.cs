using System;

namespace Wired.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    internal abstract TypeSymbol Type { get; }
}