namespace Wired.CodeAnalysis.Syntax;

public class CallExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenthesis { get; }
    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public CallExpressionSyntax(SyntaxToken identifier, SyntaxToken openParenthesis,
        SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenthesis)
    {
        Identifier = identifier;
        OpenParenthesis = openParenthesis;
        Arguments = arguments;
        CloseParenthesis = closeParenthesis;
    }
}