namespace Wired.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    // tokens
    IdentifierToken,
    NumberToken,
    StringToken,
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
    PipeToken,
    AmpersandToken,
    HatToken,
    TildeToken,
    CommaToken,

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
    CallExpression,

    // statements
    BlockStatement,
    ExpressionStatement,
    WhileStatement,
    VariableDeclarationStatement,
    IfStatement,
    ElseClause,   
    ForStatement,
}