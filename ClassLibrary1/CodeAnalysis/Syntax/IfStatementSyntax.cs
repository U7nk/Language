namespace Wired.CodeAnalysis.Syntax;

public sealed class IfStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.IfStatement;

    public IfStatementSyntax(
        SyntaxToken ifKeyword, ExpressionSyntax condition,
        StatementSyntax thenStatement, ElseClauseSyntax? elseClause)
    {
        this.IfKeyword = ifKeyword;
        this.Condition = condition;
        this.ThenStatement = thenStatement;
        this.ElseClause = elseClause;
    }

    public SyntaxToken IfKeyword { get; }
    public ExpressionSyntax Condition { get; }
    public StatementSyntax ThenStatement { get; }
    public ElseClauseSyntax? ElseClause { get; }
}