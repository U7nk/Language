namespace Wired.CodeAnalysis.Syntax;

public class BreakStatementSyntax : StatementSyntax
{
    public BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken breakKeyword, SyntaxToken semicolonToken) 
        : base(syntaxTree)
    {
        BreakKeyword = breakKeyword;
        SemicolonToken = semicolonToken;
    }
    
    public SyntaxToken BreakKeyword { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
}