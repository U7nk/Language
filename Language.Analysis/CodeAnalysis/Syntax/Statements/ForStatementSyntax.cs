namespace Language.Analysis.CodeAnalysis.Syntax;

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


    public ForStatementSyntax(SyntaxTree syntaxTree,
        SyntaxToken forKeyword, SyntaxToken openParenthesis,
        VariableDeclarationAssignmentSyntax? variableDeclaration, ExpressionSyntax? expression, 
        SyntaxToken semicolon, ExpressionSyntax condition, 
        SyntaxToken middleSemiColonToken, ExpressionSyntax mutation,
        SyntaxToken closeParenthesis, StatementSyntax body) : base(syntaxTree)
    {
        ForKeyword = forKeyword;
        OpenParenthesis = openParenthesis;
        VariableDeclaration = variableDeclaration;
        Expression = expression;
        Condition = condition;
        MiddleSemiColonToken = middleSemiColonToken;
        Mutation = mutation;
        CloseParenthesis = closeParenthesis;
        Body = body;
        Semicolon = semicolon;
    }
}