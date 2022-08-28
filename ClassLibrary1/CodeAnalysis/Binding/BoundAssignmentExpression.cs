using System;

namespace Wired.CodeAnalysis.Binding;

internal class BoundAssignmentExpression : BoundExpression
{
    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
    {
        this.Variable = variable;
        this.Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    internal override Type Type => this.Expression.Type;
}