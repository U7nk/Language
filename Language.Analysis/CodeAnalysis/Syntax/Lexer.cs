using System.Collections.Generic;
using System.Text;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class Lexer
{
    readonly SyntaxTree _syntaxTree;
    readonly SourceText _sourceText;
    readonly DiagnosticBag _diagnostics = new();

    int _position;
    int _start;
    object? _value;
    SyntaxKind _kind;
    
    public Lexer(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree;
        _sourceText = syntaxTree.SourceText;
    }


    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;
    char Current => Peek(0);
    char Lookahead => Peek(1);

    void Next(int offset = 1) 
        => _position += offset;

    SyntaxKind Next(SyntaxKind syntaxKind, int offset = 1)
    {
        Next(offset);
        return syntaxKind;
    }

    char Peek(int offset)
    {
        var index = _position + offset;
        if (index >= _sourceText.Length)
        {
            return '\0';
        }

        return _sourceText[index];
    }

    public ICollection<SyntaxToken> Lex()
    {
        var token = NextToken();
        var result = new List<SyntaxToken>();
        while (token.Kind != SyntaxKind.EndOfFileToken)
        {
            result.Add(token);
            token = NextToken();
        }

        return result;
    }

    public SyntaxToken NextToken()
    {
        _start = _position;
        _kind = SyntaxKind.BadToken;
        _value = null;


        switch (Current)
        {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                break;
            case '+':
                Next();
                _kind = SyntaxKind.PlusToken;
                break;
            case '-':
                Next();
                _kind = SyntaxKind.MinusToken;
                break;
            case '*':
                Next();
                _kind = SyntaxKind.StarToken;
                break;
            case '/':
                Next();
                _kind = SyntaxKind.SlashToken;
                break;
            case '(':
                Next();
                _kind = SyntaxKind.OpenParenthesisToken;
                break;
            case ')':
                Next();
                _kind = SyntaxKind.CloseParenthesisToken;
                break;
            case '{':
                Next();
                _kind = SyntaxKind.OpenBraceToken;
                break;
            case '}':
                Next();
                _kind = SyntaxKind.CloseBraceToken;
                break;
            case ',':
                _kind = Next(SyntaxKind.CommaToken);
                break;
            case ';':
                Next();
                _kind = SyntaxKind.SemicolonToken;
                break;
            case ':':
                _kind = Next(SyntaxKind.ColonToken);
                break;
            case '&':
                Next();
                if (Current is '&')
                    _kind = Next(SyntaxKind.AmpersandAmpersandToken);
                else
                    _kind = SyntaxKind.AmpersandToken;
                break;
            case '|':
                if (Lookahead is '|')
                    _kind = Next(SyntaxKind.PipePipeToken, 2);
                else
                    _kind = Next(SyntaxKind.PipeToken);
                break;
            case '^':
                _kind = Next(SyntaxKind.HatToken);
                break;
            case '~':
                _kind = Next(SyntaxKind.TildeToken);
                break;
            case '<':
                if (Lookahead is '=')
                    _kind = Next(SyntaxKind.LessOrEqualsToken, 2);
                else
                    _kind = Next(SyntaxKind.LessToken);
                break;
            case '>':
                Next();
                if (Current is '=')
                {
                    Next();
                    _kind = SyntaxKind.GreaterOrEqualsToken;
                }
                else
                {
                    _kind = SyntaxKind.GreaterToken;
                }
                break;
            case '=':
                Next();
                if (Current is '=')
                {
                    Next();
                    _kind = SyntaxKind.EqualsEqualsToken;
                }
                else
                {
                    _kind = SyntaxKind.EqualsToken;
                }
                break;
            case '!':
                Next();
                if (Current is '=')
                {
                    Next();
                    _kind = SyntaxKind.BangEqualsToken;
                }
                else
                {
                    _kind = SyntaxKind.BangToken;
                }
                break;
            case '.':
                _kind = Next(SyntaxKind.DotToken);
                break;
            case '"':
                ReadString();
                break;
            case '0' or '1' or '2' or '3' or '4':
            case '5' or '6' or '7' or '8' or '9':
                ReadNumberToken();
                break;
            case ' ' or '\t' or '\r' or '\n':
                ReadWhitespace();
                break;
            default:
                if (char.IsWhiteSpace(Current))
                {
                    ReadWhitespace();
                }
                else if (char.IsLetter(Current))
                {
                    ReadKeywordOrIdentifier();
                }
                else
                {
                    var  span = new TextSpan(_position, 1);
                    _diagnostics.ReportBadCharacter(new TextLocation(_sourceText, span), Current);
                    Next();
                }
                break;
        }


        var length = _position - _start;
        var text = SyntaxFacts.GetText(_kind);
        if (text is null)
        {
            text = _sourceText.ToString(_start, length);
        }

        return new SyntaxToken(_syntaxTree, _kind, _start, text, _value);
    }

    void ReadString()
    {
        var sb = new StringBuilder();
        
        LOOP_START:
        Next();
        switch (Current)
        {
            case '\0':
            case '\r':
            case '\n':
                var span = new TextSpan(_start, 1);
                var location = new TextLocation(_sourceText, span);
                _diagnostics.ReportUnterminatedString(location);
                break;
            case '"':
                Next();
                if (Current is '"')
                {
                    sb.Append(Current);
                    goto LOOP_START;
                }
                break;
            default:
                sb.Append(Current);
                goto LOOP_START;
        }


        _value = sb.ToString();
        _kind = SyntaxKind.StringToken;
    }

    void ReadKeywordOrIdentifier()
    {
        while (char.IsLetter(Current)) 
            Next();

        var text = _sourceText.ToString(_start, _position - _start);
        _kind = SyntaxFacts.GetKeywordKind(text);
    }

    void ReadWhitespace()
    {
        while (char.IsWhiteSpace(Current))
        {
            Next();
        }

        _kind = SyntaxKind.WhitespaceToken;
    }

    void ReadNumberToken()
    {
        while (char.IsDigit(Current))
        {
            Next();
        }

        var length = _position - _start;
        var text = _sourceText.ToString(_start, length);
        if (!int.TryParse(text, out var number))
        {
            var span = new TextSpan(_start, length);
            var location = new TextLocation(_sourceText, span);
            _diagnostics.ReportInvalidNumber(
                location,
                text, 
                typeof(int));
        }

        _value = number;
        _kind = SyntaxKind.NumberToken;
    }
}