namespace Language.Analysis.CodeAnalysis.Syntax;

public class NewExpressionSyntax : ExpressionSyntax
{
    public NewExpressionSyntax(SyntaxTree syntaxTree,
        SyntaxToken newKeyword, 
        SyntaxToken typeIdentifier,
        Option<GenericClauseSyntax> genericClause,
        SyntaxToken openParenthesis,
        SyntaxToken closeParenthesis) : base(syntaxTree)
    {
        NewKeyword = newKeyword;
        TypeIdentifier = typeIdentifier;
        OpenParenthesis = openParenthesis;
        CloseParenthesis = closeParenthesis;
        GenericClause = genericClause;
    }

    public SyntaxToken NewKeyword { get; }
    public SyntaxToken TypeIdentifier { get; }
    public Option<GenericClauseSyntax> GenericClause { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SyntaxToken CloseParenthesis { get; }


    public override SyntaxKind Kind => SyntaxKind.ObjectCreationExpression;
}