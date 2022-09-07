namespace Wired.CodeAnalysis.Binding;

internal class BoundIfStatement : BoundStatement
{
    public BoundExpression Condition { get; }
    public BoundStatement ThenStatement { get; }
    public BoundStatement? ElseStatement { get; }

    public BoundIfStatement(BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement)
    {
        this.Condition = condition;
        this.ThenStatement = thenStatement;
        this.ElseStatement = elseStatement;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.IfStatement;
}