using System.Collections.Generic;

namespace Wired;

public class Lexer
{
    private readonly string text;
    private int position;

    public Lexer(string text)
    {
        this.text = text;
    }


    private char Current => 
        this.position >= this.text.Length ?
            '\0' 
            :
            this.text[this.position];

    private void Next() => 
        this.position++;

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
        
        if (char.IsDigit(Current))
        {
            var start = this.position;

            while (char.IsDigit(Current))
            {
                this.Next();
            }

            var length = this.position - start;
            var text = this.text.Substring(start, length);
            int.TryParse(text, out var value);
            return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
        }

        if (char.IsWhiteSpace(Current))
        {
            var start = this.position;
            while (char.IsWhiteSpace(Current))
            {
                this.Next();
            }

            var length = this.position - start;
            var text = this.text.Substring(start, length);
            return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);
        }

        if (Current == '+')
        {
            var token = new SyntaxToken(SyntaxKind.PlusToken, this.position, "+", null);
            this.Next();
            return token;
        }
        else if (Current == '-')
        {
            var token = new SyntaxToken(SyntaxKind.MinusToken, this.position, "-", null);
            this.Next();
            return token;
        }
        else if (Current == '*')
        {
            var token = new SyntaxToken(SyntaxKind.StarToken, this.position, "*", null);
            this.Next();
            return token;
        }
        else if (Current == '/')
        {
            var token = new SyntaxToken(SyntaxKind.SlashToken, this.position, "/", null);
            this.Next();
            return token;
        }
        else if (Current == '(')
        {
            var token = new SyntaxToken(SyntaxKind.OpenParenthesisToken, this.position, "(", null);
            this.Next();
            return token;
        }
        else if (Current == ')')
        {
            var token = new SyntaxToken(SyntaxKind.CloseParenthesisToken, this.position, ")", null);
            this.Next();
            return token;
        }


        var badToken = new SyntaxToken(
            SyntaxKind.BadToken,
            this.position,
            this.text.Substring(this.position, 1),
            null);
        this.Next();
        return badToken;
    }
}