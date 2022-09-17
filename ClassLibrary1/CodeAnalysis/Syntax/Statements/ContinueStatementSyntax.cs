namespace Wired.CodeAnalysis.Syntax;

class ContinueStatementSyntax : StatementSyntax
{
    public SyntaxToken ContinueKeyword { get; }
    public SyntaxToken SemicolonToken { get; }

    public ContinueStatementSyntax(SyntaxToken continueKeyword, SyntaxToken semicolonToken)
    {
        ContinueKeyword = continueKeyword;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
}