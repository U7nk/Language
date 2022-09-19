using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Syntax;

public class BlockStatementSyntax : StatementSyntax
{
    public BlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements,
        SyntaxToken closeBraceToken) : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
    }

    public SyntaxToken OpenBraceToken { get; }
    public ImmutableArray<StatementSyntax> Statements { get; }
    public SyntaxToken CloseBraceToken { get; }

    public override SyntaxKind Kind => SyntaxKind.BlockStatement;
}