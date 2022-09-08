using System.Collections.Generic;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class Lexer
{
    private readonly SourceText sourceText;
    private int position;
    private readonly DiagnosticBag diagnostics = new();
    private int start;
    private SyntaxKind kind;
    private object? value;
    public IEnumerable<Diagnostic> Diagnostics => this.diagnostics;

    public Lexer(SourceText sourceText)
    {
        this.sourceText = sourceText;
    }


    private char Current => this.Peek(0);
    private char Lookahead => this.Peek(1);

    private void Next(int offset = 1) 
        => this.position += offset;

    private SyntaxKind Next(SyntaxKind syntaxKind, int offset = 1)
    {
        this.Next(offset);
        return syntaxKind;
    }

    private char Peek(int offset)
    {
        var index = this.position + offset;
        if (index >= this.sourceText.Length)
        {
            return '\0';
        }

        return this.sourceText[index];
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
        this.start = this.position;
        this.kind = SyntaxKind.BadToken;
        this.value = null;


        switch (this.Current)
        {
            case '\0':
                this.kind = SyntaxKind.EndOfFileToken;
                break;
            case '+':
                this.Next();
                this.kind = SyntaxKind.PlusToken;
                this.kind = (SyntaxKind.PlusToken);
                break;
            case '-':
                this.Next();
                this.kind = SyntaxKind.MinusToken;
                break;
            case '*':
                this.Next();
                this.kind = SyntaxKind.StarToken;
                break;
            case '/':
                this.Next();
                this.kind = SyntaxKind.SlashToken;
                break;
      
            case '(':
                this.Next();
                this.kind = SyntaxKind.OpenParenthesisToken;
                break;
            case ')':
                this.Next();
                this.kind = SyntaxKind.CloseParenthesisToken;
                break;
            case '{':
                this.Next();
                this.kind = SyntaxKind.OpenBraceToken;
                break;
            case '}':
                this.Next();
                this.kind = SyntaxKind.CloseBraceToken;
                break;
            case ';':
                this.Next();
                this.kind = SyntaxKind.SemicolonToken;
                break;
            case '&':
                this.Next();
                if (this.Current is '&')
                    this.kind = this.Next(SyntaxKind.AmpersandAmpersandToken);
                else
                    this.kind = SyntaxKind.AmpersandToken;
                break;
            case '|':
                if (this.Lookahead is '|')
                    this.kind = this.Next(SyntaxKind.PipePipeToken, 2);
                else
                    this.kind = this.Next(SyntaxKind.PipeToken);
                break;
            case '^':
                this.kind = this.Next(SyntaxKind.HatToken);
                break;
            case '~':
                this.kind = this.Next(SyntaxKind.TildeToken);
                break;
            case '<':
                if (this.Lookahead is '=')
                    this.kind = this.Next(SyntaxKind.LessOrEqualsToken, 2);
                else
                    this.kind = this.Next(SyntaxKind.LessToken);
                break;
            case '>':
                this.Next();
                if (this.Current is '=')
                {
                    this.Next();
                    this.kind = SyntaxKind.GreaterOrEqualsToken;
                }
                else
                {
                    this.kind = SyntaxKind.GreaterToken;
                }
                break;
            case '=':
                this.Next();
                if (this.Current is '=')
                {
                    this.Next();
                    this.kind = SyntaxKind.EqualsEqualsToken;
                }
                else
                {
                    this.kind = SyntaxKind.EqualsToken;
                }
                break;
            case '!':
                this.Next();
                if (this.Current is '=')
                {
                    this.Next();
                    this.kind = SyntaxKind.BangEqualsToken;
                }
                else
                {
                    this.kind = SyntaxKind.BangToken;
                }
                break;
            case '0' or '1' or '2' or '3' or '4':
            case '5' or '6' or '7' or '8' or '9':
                this.ReadNumberToken();
                break;
            case ' ' or '\t' or '\r' or '\n':
                this.ReadWhitespace();
                break;
            default:
                if (char.IsWhiteSpace(this.Current))
                {
                    this.ReadWhitespace();
                }
                else if (char.IsLetter(this.Current))
                {
                    this.ReadKeywordOrIdentifier();
                }
                else
                {
                    this.diagnostics.ReportBadCharacter(this.position, this.Current);
                    this.Next();
                }
                break;
        }


        var length = this.position - this.start;
        var text = SyntaxFacts.GetText(this.kind);
        if (text is null)
        {
            text = this.sourceText.ToString(this.start, length);
        }

        return new SyntaxToken(this.kind, this.start, text, this.value);
    }

    private void ReadKeywordOrIdentifier()
    {
        while (char.IsLetter(this.Current)) 
            this.Next();

        var text = this.sourceText.ToString(this.start, this.position - this.start);
        this.kind = SyntaxFacts.GetKeywordKind(text);
    }

    private void ReadWhitespace()
    {
        while (char.IsWhiteSpace(this.Current))
        {
            this.Next();
        }

        this.kind = SyntaxKind.WhitespaceToken;
    }

    private void ReadNumberToken()
    {
        while (char.IsDigit(this.Current))
        {
            this.Next();
        }

        var length = this.position - this.start;
        var text = this.sourceText.ToString(this.start, length);
        if (!int.TryParse(text, out var number))
        {
            this.diagnostics.ReportInvalidNumber(this.start, length, text, typeof(int));
        }

        this.value = number;
        this.kind = SyntaxKind.NumberToken;
    }
}