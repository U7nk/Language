namespace Wired.CodeAnalysis.Syntax;

class BreakStatementSyntax : StatementSyntax
{
    public BreakStatementSyntax(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
    {
        BreakKeyword = breakKeyword;
        SemicolonToken = semicolonToken;
    }
    
    public SyntaxToken BreakKeyword { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
}