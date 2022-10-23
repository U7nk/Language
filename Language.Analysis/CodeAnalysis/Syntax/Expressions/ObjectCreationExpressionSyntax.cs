namespace Language.Analysis.CodeAnalysis.Syntax;

public class ObjectCreationExpressionSyntax : ExpressionSyntax
{
    public ObjectCreationExpressionSyntax(SyntaxTree syntaxTree,
        SyntaxToken newKeyword, 
        SyntaxToken typeIdentifier, 
        SyntaxToken openParenthesis,
        SyntaxToken closeParenthesis) : base(syntaxTree)
    {
        NewKeyword = newKeyword;
        TypeIdentifier = typeIdentifier;
        OpenParenthesis = openParenthesis;
        CloseParenthesis = closeParenthesis;
    }

    public SyntaxToken NewKeyword { get; }
    public SyntaxToken TypeIdentifier { get; }
    public SyntaxToken OpenParenthesis { get; }
    public SyntaxToken CloseParenthesis { get; }


    public override SyntaxKind Kind => SyntaxKind.ObjectCreationExpression;
}