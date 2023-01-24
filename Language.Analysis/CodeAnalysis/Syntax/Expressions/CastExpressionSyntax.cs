namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class CastExpressionSyntax : ExpressionSyntax
{
    public CastExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken openParenthesis, NameExpressionSyntax nameExpression, SyntaxToken closeParenthesis, ExpressionSyntax castedExpression) 
        : base(syntaxTree)
    {
        OpenParenthesis = openParenthesis;
        NameExpression = nameExpression;
        CloseParenthesis = closeParenthesis;
        CastedExpression = castedExpression;
    }

    public SyntaxToken OpenParenthesis { get; }
    public NameExpressionSyntax NameExpression { get; }
    public SyntaxToken CloseParenthesis { get; }
    public ExpressionSyntax CastedExpression { get; }

    public override SyntaxKind Kind => SyntaxKind.CastExpression;
}