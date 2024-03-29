namespace Language.Analysis.CodeAnalysis.Syntax;

public class ContinueStatementSyntax : StatementSyntax
{
    public SyntaxToken ContinueKeyword { get; }
    public SyntaxToken SemicolonToken { get; }

    public ContinueStatementSyntax(SyntaxTree syntaxTree,SyntaxToken continueKeyword, SyntaxToken semicolonToken) 
        : base(syntaxTree)
    {
        ContinueKeyword = continueKeyword;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
}