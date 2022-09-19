namespace Wired.CodeAnalysis.Syntax;

public class ReturnStatementSyntax : StatementSyntax
{
    public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression,
        SyntaxToken semicolon)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
        Semicolon = semicolon;
    }

    public ExpressionSyntax? Expression { get; }

    public SyntaxToken ReturnKeyword { get; }
    public SyntaxToken Semicolon { get; }

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
}