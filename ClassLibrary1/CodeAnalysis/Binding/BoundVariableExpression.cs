using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundVariableExpression : BoundExpression
{
    public VariableSymbol Variable { get; }

    public BoundVariableExpression(VariableSymbol variable)
    {
        this.Variable = variable;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    internal override TypeSymbol Type => this.Variable.Type;
}