namespace Wired.CodeAnalysis.Syntax;

class GlobalStatementSyntax : MemberSyntax
{
    public GlobalStatementSyntax(StatementSyntax statement)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public StatementSyntax Statement { get; }
}