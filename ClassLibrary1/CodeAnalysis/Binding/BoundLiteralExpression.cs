using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundLiteralExpression : BoundExpression
{
    internal override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    internal override TypeSymbol Type { get; }
    internal object? Value { get; }
    internal BoundLiteralExpression(object? value, TypeSymbol type)
    {
        this.Value = value;
        this.Type = type;
    }
}