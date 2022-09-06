namespace Wired.CodeAnalysis.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseClause;

    public ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax elseStatement)
    {
        this.ElseKeyword = elseKeyword;
        this.ElseStatement = elseStatement;
    }

    public SyntaxToken ElseKeyword { get; }
    public StatementSyntax ElseStatement { get; }
}