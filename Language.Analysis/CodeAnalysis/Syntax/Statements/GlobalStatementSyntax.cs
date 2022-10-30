namespace Language.Analysis.CodeAnalysis.Syntax;

public class GlobalStatementDeclarationSyntax : SyntaxNode, ITopMemberDeclarationSyntax
{
    public GlobalStatementDeclarationSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public StatementSyntax Statement { get; }
}