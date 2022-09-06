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
    
    // nodes
    CompilationUnit,

    // expressions
    LiteralExpression,
    BinaryExpression,
    ParenthesizedExpression,
    UnaryExpression,
    AssignmentExpression,
    NameExpression,
    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    ElseClause,
    IfKeyword,
    ElseKeyword
}