namespace Language.Analysis.CodeAnalysis.Syntax;

public class VariableDeclarationAssignmentStatementSyntax : StatementSyntax
{
    public VariableDeclarationAssignmentSyntax VariableDeclaration { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationAssignmentStatement;

    public VariableDeclarationAssignmentStatementSyntax(
        SyntaxTree syntaxTree, VariableDeclarationAssignmentSyntax variableDeclaration,
        SyntaxToken semicolonToken) : base(syntaxTree)
    {
        VariableDeclaration = variableDeclaration;
        SemicolonToken = semicolonToken;
    }
}