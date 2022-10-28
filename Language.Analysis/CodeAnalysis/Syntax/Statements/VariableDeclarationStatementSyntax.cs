namespace Language.Analysis.CodeAnalysis.Syntax;

public class VariableDeclarationStatementSyntax : StatementSyntax
{
    public VariableDeclarationSyntax VariableDeclaration { get; }
    public SyntaxToken SemicolonToken { get; }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;

    public VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, VariableDeclarationSyntax variableDeclaration,
        SyntaxToken semicolonToken) : base(syntaxTree)
    {
        VariableDeclaration = variableDeclaration;
        SemicolonToken = semicolonToken;
    }
}