namespace Language.Analysis.CodeAnalysis.Syntax;

public class MemberAssignmentExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax MemberAccess { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Initializer { get; }

    public MemberAssignmentExpressionSyntax(
        SyntaxTree syntaxTree,
        ExpressionSyntax memberAccess,
        SyntaxToken equalsToken,
        ExpressionSyntax initializer) : base(syntaxTree)
    {
        MemberAccess = memberAccess;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }

    public override SyntaxKind Kind => SyntaxKind.MemberAssignmentExpression;
}