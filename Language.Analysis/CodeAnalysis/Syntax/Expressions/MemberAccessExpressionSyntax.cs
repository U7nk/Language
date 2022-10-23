namespace Language.Analysis.CodeAnalysis.Syntax;

public class MemberAccessExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken Dot { get; }
    public ExpressionSyntax Right { get; }

    public MemberAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken dot, ExpressionSyntax right) : base(syntaxTree)
    {
        Left = left;
        Dot = dot;
        Right = right;
    }

    public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;
}