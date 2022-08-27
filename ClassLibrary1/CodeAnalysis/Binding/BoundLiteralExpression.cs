using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundLiteralExpression : BoundExpression
{
    internal override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    internal override Type Type => this.Value.GetType();
    internal object Value { get; }
    internal BoundLiteralExpression(object value)
    {
        this.Value = value;
    }
}