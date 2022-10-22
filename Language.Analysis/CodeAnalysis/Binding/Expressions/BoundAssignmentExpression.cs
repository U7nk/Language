using System;
using Language.CodeAnalysis.Symbols;
using Language.CodeAnalysis.Syntax;

namespace Language.CodeAnalysis.Binding;

internal class BoundAssignmentExpression : BoundExpression
{
    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
    {
        Variable = variable;
        Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    internal override TypeSymbol Type => Expression.Type;
}