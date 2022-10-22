namespace Language.Analysis.CodeAnalysis.Syntax;

public class FieldAssignmentExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax ObjectAccess { get; }
    public SyntaxToken FieldIdentifier { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Initializer { get; }

    public FieldAssignmentExpressionSyntax(SyntaxTree syntaxTree,
        ExpressionSyntax objectAccess,
        SyntaxToken fieldIdentifier,
        SyntaxToken equalsToken,
        ExpressionSyntax initializer) : base(syntaxTree)
    {
        ObjectAccess = objectAccess;
        FieldIdentifier = fieldIdentifier;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }

    public override SyntaxKind Kind => SyntaxKind.FieldAssignmentExpression;
}