using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public class Lexer
{
    private readonly string text;
    private int position;
    private List<string> diagnostics = new List<string>();
    public IEnumerable<string> Diagnostics => this.diagnostics;

    public Lexer(string text)
    {
        this.text = text;
    }


    private char Current => Peek(0);
    private char Lookahead => Peek(1);

    private void Next(int offset = 1)
    {
        this.position += offset;
    }

    private char Peek(int offset)
    {
        var index = this.position + offset;
        if (index >= this.text.Length)
        {
            return '\0';
        }

        return this.text[index];
    }

    public ICollection<SyntaxToken> Parse()
    {
        var token = this.NextToken();
        var result = new List<SyntaxToken>();
        while (token.Kind != SyntaxKind.EndOfFileToken)
        {
            result.Add(token);
            token = this.NextToken();
        }

        return result;
    }

    public SyntaxToken NextToken()
    {
        if (this.position >= this.text.Length)
        {
            return new SyntaxToken(SyntaxKind.EndOfFileToken, this.position, "\0", null);
        }

        if (char.IsDigit(this.Current))
        {
            var start = this.position;

            while (char.IsDigit(this.Current))
            {
                this.Next();
            }

            var length = this.position - start;
            var text = this.text.Substring(start, length);
            if (!int.TryParse(text, out var value))
            {
                this.diagnostics.Add($"error: The number {text} cannot be represented by Int32.");
            }

            return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
        }

        if (char.IsLetter(Current))
        {
            var start = this.position;
            while (char.IsLetter(Current))
            {
                this.Next();
            }

            var letters = this.text[start..this.position];
            var kind = SyntaxFacts.GetKeywordKind(letters);
            return new SyntaxToken(kind, start, letters, null);
        }

        if (char.IsWhiteSpace(this.Current))
        {
            var start = this.position;
            while (char.IsWhiteSpace(this.Current))
            {
                this.Next();
            }

            var length = this.position - start;
            var text = this.text.Substring(start, length);
            return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);
        }

        switch (this.Current)
        {
            case '+':
            {
                var token = new SyntaxToken(SyntaxKind.PlusToken, this.position, "+", null);
                this.Next();
                return token;
            }
            case '-':
            {
                var token = new SyntaxToken(SyntaxKind.MinusToken, this.position, "-", null);
                this.Next();
                return token;
            }
            case '*':
            {
                var token = new SyntaxToken(SyntaxKind.StarToken, this.position, "*", null);
                this.Next();
                return token;
            }
            case '/':
            {
                var token = new SyntaxToken(SyntaxKind.SlashToken, this.position, "/", null);
                this.Next();
                return token;
            }
            case '(':
            {
                var token = new SyntaxToken(SyntaxKind.OpenParenthesisToken, this.position, "(", null);
                this.Next();
                return token;
            }
            case ')':
            {
                var token = new SyntaxToken(SyntaxKind.CloseParenthesisToken, this.position, ")", null);
                this.Next();
                return token;
            }
            case '!':
            {
                var token = new SyntaxToken(SyntaxKind.BangToken, this.position, "!", null);
                this.Next();
                return token;
            }
            case '&':
                if (this.Lookahead is '&')
                {
                    var token = new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, this.position, "&&", null);
                    this.Next(2);
                    return token;
                }
                break;
            case '|':
                if (this.Lookahead is '|')
                {
                    var token = new SyntaxToken(SyntaxKind.PipePipeToken, this.position, "||", null);
                    this.Next(2);
                    return token;
                }
                break;
        }

        this.diagnostics.Add($"error: bad character '{this.Current}'");
        var badToken = new SyntaxToken(
            SyntaxKind.BadToken,
            this.position,
            this.text.Substring(this.position, 1),
            null);
        this.Next();
        return badToken;
    }
}