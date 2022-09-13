using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundVariableExpression : BoundExpression
{
    public VariableSymbol Variable { get; }

    public BoundVariableExpression(VariableSymbol variable)
    {
        Variable = variable;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    internal override TypeSymbol Type => Variable.Type;
}