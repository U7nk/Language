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
    
    // keywords
    TrueKeyword,
    FalseKeyword,

    // expressions
    LiteralExpression,
    BinaryExpression,
    ParenthesizedExpression,
    UnaryExpression,
    AssignmentExpression,
    NameExpression
}