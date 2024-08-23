namespace Language.Analysis.CodeAnalysis.Syntax;

public class NewExpressionSyntax : ExpressionSyntax
{
    public NewExpressionSyntax(SyntaxTree syntaxTree,
        SyntaxToken newKeyword, 
        NamedTypeExpressionSyntax namedTypeExpression,
        SyntaxToken openParenthesis,
        SyntaxToken closeParenthesis) : base(syntaxTree)
    {
        NewKeyword = newKeyword;
        NamedTypeExpression = namedTypeExpression;
        OpenParenthesis = openParenthesis;
        CloseParenthesis = closeParenthesis;
    }

    public SyntaxToken NewKeyword { get; }
    public NamedTypeExpressionSyntax NamedTypeExpression { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SyntaxToken CloseParenthesis { get; }


    public override SyntaxKind Kind => SyntaxKind.ObjectCreationExpression;
}