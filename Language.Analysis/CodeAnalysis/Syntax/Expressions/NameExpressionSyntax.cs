namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier) : base(syntaxTree)
    {
        Identifier = identifier;
    }

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public SyntaxToken Identifier { get; }
}