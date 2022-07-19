namespace Wired;

public enum SyntaxKind
{
    NumberToken,
    WhitespaceToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    BadToken,
    EndOfFileToken
}

public class SyntaxToken
{
    public SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    
    public SyntaxToken(SyntaxKind kind,int position, string text, object? value)
    {
        Kind = kind;
        Position = position;
        Text = text;
        this.Value = value;
    }
}