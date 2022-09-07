namespace Wired.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    // tokens
    IdentifierToken,
    NumberToken,
    WhitespaceToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    BadToken,
    EndOfFileToken,
    BangToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    EqualsEqualsToken,
    BangEqualsToken,
    EqualsToken,
    OpenBraceToken,
    CloseBraceToken,
    SemicolonToken,
    LessOrEqualsToken,
    LessToken,
    GreaterOrEqualsToken,
    GreaterToken,

    
    // keywords
    TrueKeyword,
    FalseKeyword,
    VarKeyword,
    LetKeyword,
    WhileKeyword,
    IfKeyword,
    ElseKeyword,
    ForKeyword,
    
    // nodes
    CompilationUnit,
    VariableDeclarationSyntax,
    VariableDeclarationAssignmentSyntax,
    
    // expressions
    LiteralExpression,
    BinaryExpression,
    ParenthesizedExpression,
    UnaryExpression,
    AssignmentExpression,
    NameExpression,

    // statements
    BlockStatement,
    ExpressionStatement,
    WhileStatement,
    VariableDeclarationStatement,
    IfStatement,
    ElseClause,   
    ForStatement,
}