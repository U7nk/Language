namespace Wired.CodeAnalysis.Syntax;

public class VariableDeclarationAssignmentSyntax : SyntaxNode
{
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationAssignmentSyntax;

    public VariableDeclarationAssignmentSyntax(SyntaxTree syntaxTree, VariableDeclarationSyntax variableDeclaration, SyntaxToken equalsToken,
        ExpressionSyntax expression) 
        : base(syntaxTree)
    {
        VariableDeclaration = variableDeclaration;
        EqualsToken = equalsToken;
        Initializer = expression;
    }
    public VariableDeclarationSyntax VariableDeclaration { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Initializer { get; }
}