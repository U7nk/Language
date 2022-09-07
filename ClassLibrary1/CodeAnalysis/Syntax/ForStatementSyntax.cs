namespace Wired.CodeAnalysis.Syntax;

public sealed class ForStatementSyntax : StatementSyntax
{
    public override SyntaxKind Kind => SyntaxKind.ForStatement;
    public SyntaxToken ForKeyword { get; }
    public SyntaxToken OpenParenthesis { get; }
    public VariableDeclarationAssignmentSyntax? VariableDeclaration { get; }
    public ExpressionSyntax? Expression { get; }
    public SyntaxToken Semicolon { get; }
    public ExpressionSyntax Condition { get; }
    public SyntaxToken MiddleSemiColonToken { get; }
    public ExpressionSyntax Mutation { get; }
    public SyntaxToken CloseParenthesis { get; }
    public StatementSyntax Body { get; }


    public ForStatementSyntax(
        SyntaxToken forKeyword, SyntaxToken openParenthesis,
        VariableDeclarationAssignmentSyntax? variableDeclaration, ExpressionSyntax? expression, 
        SyntaxToken semicolon, ExpressionSyntax condition, 
        SyntaxToken middleSemiColonToken, ExpressionSyntax mutation,
        SyntaxToken closeParenthesis, StatementSyntax body)
    {
        this.ForKeyword = forKeyword;
        this.OpenParenthesis = openParenthesis;
        this.VariableDeclaration = variableDeclaration;
        this.Expression = expression;
        this.Condition = condition;
        this.MiddleSemiColonToken = middleSemiColonToken;
        this.Mutation = mutation;
        this.CloseParenthesis = closeParenthesis;
        this.Body = body;
        this.Semicolon = semicolon;
    }
}