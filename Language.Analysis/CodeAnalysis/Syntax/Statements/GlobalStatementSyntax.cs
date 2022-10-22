namespace Language.CodeAnalysis.Syntax;

public class GlobalStatementSyntax : SyntaxNode, ITopMemberSyntax
{
    public GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public StatementSyntax Statement { get; }
}