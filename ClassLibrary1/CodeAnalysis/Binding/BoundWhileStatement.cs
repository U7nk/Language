namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundWhileStatement : BoundStatement
{
  internal override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

  public BoundWhileStatement(BoundExpression condition, BoundStatement body)
  {
    Condition = condition;
    Body = body;
  }
  public BoundExpression Condition { get; }
  public BoundStatement Body { get; }
}