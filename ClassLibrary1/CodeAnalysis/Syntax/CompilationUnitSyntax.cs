namespace Wired.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken endOfFileToken)
    {
        this.Statement = statement;
        this.EndOfFileToken = endOfFileToken;
    }

    public StatementSyntax Statement { get; }
    public SyntaxToken EndOfFileToken { get; }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}