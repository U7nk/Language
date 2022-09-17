namespace Wired.CodeAnalysis.Binding;


abstract class BoundLoopStatement : BoundStatement
{
    protected BoundLoopStatement(LabelSymbol breakLabel, LabelSymbol continueLabel)
    {
        BreakLabel = breakLabel;
        ContinueLabel = continueLabel;
    }
    
    public LabelSymbol BreakLabel { get; set; }
    public LabelSymbol ContinueLabel { get; set; }
    
}

internal sealed class BoundWhileStatement : BoundLoopStatement
{
  internal override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
  
  public BoundWhileStatement(BoundExpression condition, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel) 
      : base(breakLabel, continueLabel)
  {
    Condition = condition;
    Body = body;
  }
  
  public BoundExpression Condition { get; }
  public BoundStatement Body { get; }
}