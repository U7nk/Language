using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Wired
{

    public class TestContext
    {
        public Type Type { get; set; }
        public string MemberName { get; set; }

        public List<List<int>> ListOfList { get; set; }

        public bool IntEquals(int left, int right)
        {
            return left == right;
        }

        public bool ObjectEquals(object left, object right)
        {
            return left == right;
        }

        public bool StringEquals(object left, object right)
        {
            return String.Equals(left, right);
        }

        public string OverloadedMethod(string str)
        {
            return str;
        }

        public int OverloadedMethod(int number)
        {
            return number;
        }

        public bool FuncTwoParams(int fParam, Func<int, int, bool> funcTwoParams, int sParam)
        {
            return funcTwoParams(fParam, sParam);
        }
    }

    public enum TokenType
    {
        Id,
        OpFrame,
        ClFrame,
        Dot,
        Comma,
        Question,
        Colon,
        EOF,
        StringLiteral,
        IntLiteral,
        DoubleLiteral,
        FloatLiteral,
        Plus,
        BinaryGreater,
        Minus,
        Div,
        Mul,
        CtxKeyword,
        TrueKeyword,
        FalseKeyword,
        LambdaArrow,
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string StringValue { get; set; }
        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.StringValue = value;
        }


        public bool Equals(Token other)
        {
            return this.Type == other.Type && this.StringValue == other.StringValue;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((Token)obj);
        }
        public override int GetHashCode()
        {
            return StringValue.GetHashCode() + Type.GetHashCode();
        }
    }

    public sealed class Tokenizer
    {
        private readonly string input;

        public Tokenizer(string input)
        {
            this.input = input;
            this.Keywords = new List<string> { "$ctx", "true", "false" };
        }

        public IEnumerable<string> Keywords { get; set; } 
        private Token TokenFromIdentifier(string identifier)
        {
            if (IsKeyword(identifier))
            {
                return TokenFromKeyword(identifier);
            }

            return new Token(TokenType.Id, identifier);
        }
        private Token TokenFromKeyword(string identifier)
        {
            if (identifier == "$ctx")
            {
                return new Token(TokenType.CtxKeyword, "$ctx");
            }
            else if (identifier == "true")
            {
                return new Token(TokenType.TrueKeyword, "true");
            }
            else if (identifier == "false")
            {
                return new Token(TokenType.FalseKeyword, "false");
            }
            throw new Exception("Undefined keyword");
        }

        private bool IsKeyword(string identifier)
        {
            if (Keywords.Contains(identifier))
            {
                return true;
            }
            return false;
        }
        public List<Token> Tokenize()
        {
            var result = new List<Token>();
            for (int i = 0; i < input.Length; i++)
            {
                var ch = this.input[i];
                if (char.IsLetter(ch) || ch == '$')
                {
                    var identifier = "";
                    while (char.IsLetter(ch) || char.IsDigit(ch) || ch == '$')
                    {
                        identifier += ch;
                        i++;
                        if (i >= this.input.Length)
                        {
                            break;
                        }
                        ch = this.input[i];
                    }
                    result.Add(TokenFromIdentifier(identifier));
                    i--;
                    continue;
                }
                if (ch == '\'' ||ch == '"')
                {
                    i++;
                    ch = this.input[i];
                    var literalValue = "";
                    while (ch != '\'' && ch != '"')
                    {
                        literalValue += ch;
                        i++;
                        ch = this.input[i];
                    }
                    result.Add(new Token(TokenType.StringLiteral, literalValue));
                    continue;
                }
                if (char.IsDigit(ch))
                {
                    var literalValue = "";
                    while (char.IsDigit(ch))
                    {
                        literalValue += ch;
                        i++;
                        if (i >= input.Length)
                        {
                            break;
                        }
                        ch = this.input[i];
                    }
                    if (ch == '.')
                    {
                        i++;
                        ch = this.input[i];
                        if (!char.IsDigit(ch))
                        {
                            result.Add(new Token(TokenType.IntLiteral, literalValue));
                            i -= 2;
                            continue;
                        }
                        literalValue += ','; // not dot because of float.Parse or double.Parse which will be used to evaluate value of number // культуру смени и нормально сделай
                        while (char.IsDigit(ch))
                        {
                            literalValue += ch;
                            i++;
                            if (i >= input.Length)
                            {
                                break;
                            }
                            ch = this.input[i];
                        }
                        if (ch == 'f')
                        {
                            result.Add(new Token(TokenType.FloatLiteral, literalValue));
                            continue;
                        }
                        i--;
                        result.Add(new Token(TokenType.DoubleLiteral, literalValue));
                        continue;
                    }
                    i--;
                    result.Add(new Token(TokenType.IntLiteral, literalValue));
                    continue;
                }
                if (ch == '(')
                {
                    result.Add(new Token(TokenType.OpFrame, "("));
                    continue;
                }
                if (ch == ')')
                {
                    result.Add(new Token(TokenType.ClFrame, ")"));
                    continue;
                }
                if (ch == '.')
                {
                    result.Add(new Token(TokenType.Dot, "."));
                    continue;
                }
                if (ch == ',')
                {
                    result.Add(new Token(TokenType.Comma, ","));
                    continue;
                }
                if (ch == '?')
                {
                    result.Add(new Token(TokenType.Question, "?"));
                    continue;
                }
                if (ch == ':')
                {
                    result.Add(new Token(TokenType.Colon, ":"));
                    continue;
                }
                if (ch == '+')
                {
                    result.Add(new Token(TokenType.Plus, "+"));
                    continue;
                }
                if (ch == '>')
                {
                    result.Add(new Token(TokenType.BinaryGreater, ">"));
                    continue;
                }
                if (ch == '-')
                {
                    result.Add(new Token(TokenType.Minus, "-"));
                    continue;
                }
                if (ch == '*')
                {
                    result.Add(new Token(TokenType.Mul, "*"));
                    continue;
                }
                if (ch == '/')
                {
                    result.Add(new Token(TokenType.Div, "/"));
                    continue;
                }
                if (ch == '=')
                {
                    i++;
                    ch = this.input[i];
                    if (ch == '>')
                    {
                        result.Add(new Token(TokenType.LambdaArrow, "=>"));
                        continue;
                    }
                    throw new Exception();
                }
            }
            result.Add(new Token(TokenType.EOF, ""));
            return result;
        }
    }
}
