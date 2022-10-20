namespace Wired.CodeAnalysis.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
  public override SyntaxKind Kind => SyntaxKind.WhileStatement;
  
    public WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body) 
        : base(syntaxTree)
    {
        WhileKeyword = whileKeyword;
        Condition = condition;
        Body = body;
    }

    public StatementSyntax Body { get; }

    public ExpressionSyntax Condition { get; }

    public SyntaxToken WhileKeyword { get; }
}