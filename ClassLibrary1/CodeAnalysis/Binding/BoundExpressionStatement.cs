namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpression Expression { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpressionStatement(BoundExpression expression)
    {
        this.Expression = expression;
    }
}