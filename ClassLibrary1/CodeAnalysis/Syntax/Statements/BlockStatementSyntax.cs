using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Syntax;

public class BlockStatementSyntax : StatementSyntax
{
    public BlockStatementSyntax(SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements,
        SyntaxToken closeBraceToken)
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