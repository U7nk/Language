namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationStatementSyntax : StatementSyntax
{
    public VariableDeclarationAssignmentSyntax VariableDeclaration { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;

    public VariableDeclarationStatementSyntax(
        VariableDeclarationAssignmentSyntax variableDeclaration, SyntaxToken semicolonToken)
    {
        VariableDeclaration = variableDeclaration;
        SemicolonToken = semicolonToken;
    }
}