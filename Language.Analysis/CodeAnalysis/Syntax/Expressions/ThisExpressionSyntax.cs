namespace Language.Analysis.CodeAnalysis.Syntax;

public class ThisExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken ThisKeyword { get; }

    public ThisExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken thisKeyword) : base(syntaxTree)
    {
        ThisKeyword = thisKeyword;
    }

    public override SyntaxKind Kind => SyntaxKind.ThisExpression;
}