using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;


abstract class BoundLoopStatement : BoundStatement
{
    protected BoundLoopStatement(Option<SyntaxNode> syntax, LabelSymbol loopBreakLabel, LabelSymbol loopStartLabel) : base(syntax)
    {
        LoopBreakLabel = loopBreakLabel;
        LoopStartLabel = loopStartLabel;
    }
    
    public LabelSymbol LoopBreakLabel { get; set; }
    public LabelSymbol LoopStartLabel { get; set; }
    
}

internal sealed class BoundWhileStatement : BoundLoopStatement
{
  internal override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
  
  public BoundWhileStatement(Option<SyntaxNode> syntax,BoundExpression condition, BoundStatement body, LabelSymbol loopBreakLabel, LabelSymbol loopStartLabel) 
      : base(syntax, loopBreakLabel, loopStartLabel)
  {
    Condition = condition;
    Body = body;
  }
  
  public BoundExpression Condition { get; }
  public BoundStatement Body { get; }
}