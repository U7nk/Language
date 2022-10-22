namespace Language.CodeAnalysis.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.ElseClause;

    public ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax elseStatement) 
        : base(syntaxTree)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public SyntaxToken ElseKeyword { get; }
    public StatementSyntax ElseStatement { get; }
}