namespace Language.CodeAnalysis.Syntax;

public class VariableDeclarationStatementSyntax : StatementSyntax
{
    public VariableDeclarationAssignmentSyntax VariableDeclaration { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;

    public VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, VariableDeclarationAssignmentSyntax variableDeclaration,
        SyntaxToken semicolonToken) : base(syntaxTree)
    {
        VariableDeclaration = variableDeclaration;
        SemicolonToken = semicolonToken;
    }
}