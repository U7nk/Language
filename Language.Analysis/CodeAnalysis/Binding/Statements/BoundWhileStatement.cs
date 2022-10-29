using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;


abstract class BoundLoopStatement : BoundStatement
{
    protected BoundLoopStatement(SyntaxNode? syntax, LabelSymbol breakLabel, LabelSymbol continueLabel) : base(syntax)
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
  
  public BoundWhileStatement(SyntaxNode? syntax,BoundExpression condition, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel) 
      : base(syntax, breakLabel, continueLabel)
  {
    Condition = condition;
    Body = body;
  }
  
  public BoundExpression Condition { get; }
  public BoundStatement Body { get; }
}