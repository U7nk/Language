namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationAssignmentSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationAssignmentSyntax;

    public VariableDeclarationAssignmentSyntax(VariableDeclarationSyntax variableDeclaration, SyntaxToken equalsToken, ExpressionSyntax expression)
    {
        this.VariableDeclaration = variableDeclaration;
        this.EqualsToken = equalsToken;
        this.Initializer = expression;
    }
    public VariableDeclarationSyntax VariableDeclaration { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Initializer { get; }
}