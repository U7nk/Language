namespace Language.Analysis.CodeAnalysis.Syntax;

public class MemberAssignmentExpressionSyntax : ExpressionSyntax
{
    public MemberAccessExpressionSyntax MemberAccess { get; }
    public ExpressionSyntax Initializer { get; }

    public MemberAssignmentExpressionSyntax(
        SyntaxTree syntaxTree,
        MemberAccessExpressionSyntax memberAccess,
        ExpressionSyntax initializer) : base(syntaxTree)
    {
        MemberAccess = memberAccess;
        Initializer = initializer;
    }

    public override SyntaxKind Kind => SyntaxKind.MemberAssignmentExpression;
}