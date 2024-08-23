namespace Language.Analysis.CodeAnalysis.Syntax;

public class GlobalStatementSyntax : SyntaxNode, IGlobalMemberSyntax
{
    public GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public StatementSyntax Statement { get; }
}