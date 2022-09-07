namespace Wired.CodeAnalysis.Syntax;

internal sealed class WhileStatementSyntax : StatementSyntax
{
  public override SyntaxKind Kind => SyntaxKind.WhileStatement;
  
    public WhileStatementSyntax(SyntaxToken whileKeyword, ExpressionSyntax condition, StatementSyntax body)
    {
        WhileKeyword = whileKeyword;
        Condition = condition;
        Body = body;
    }

    public StatementSyntax Body { get; }

    public ExpressionSyntax Condition { get; }

    public SyntaxToken WhileKeyword { get; }
}