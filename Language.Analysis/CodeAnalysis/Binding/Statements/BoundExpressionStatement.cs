namespace Language.CodeAnalysis.Binding;

internal sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpression Expression { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpressionStatement(BoundExpression expression)
    {
        Expression = expression;
    }
}