namespace Language.Analysis.CodeAnalysis.Syntax;

public class MethodCallExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken Identifier { get; }
    public Option<GenericClauseSyntax> GenericClause { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenthesis { get; }
    public override SyntaxKind Kind => SyntaxKind.MethodCallExpression;

    public MethodCallExpressionSyntax(SyntaxTree syntaxTree, 
        SyntaxToken identifier,
        Option<GenericClauseSyntax> genericClause,
        SyntaxToken openParenthesis,
        SeparatedSyntaxList<ExpressionSyntax> arguments,
        SyntaxToken closeParenthesis) : base(syntaxTree)
    {
        Identifier = identifier;
        GenericClause = genericClause;
        OpenParenthesis = openParenthesis;
        Arguments = arguments;
        CloseParenthesis = closeParenthesis;
    }
}