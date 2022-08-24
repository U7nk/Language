namespace Wired.CodeAnalysis;

public enum SyntaxKind
{
    // tokens
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
    
    // expressions
    LiteralExpression,
    BinaryExpression,
    ParenthesizedExpression
}