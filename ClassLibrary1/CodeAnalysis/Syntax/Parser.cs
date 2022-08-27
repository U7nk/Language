using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public class Parser
{
    public int position;
    public SyntaxToken[] tokens;
    private List<string> diagnostics = new();
    public IEnumerable<string> Diagnostics => this.diagnostics;

    public Parser(string text)
    {
        var lexer = new Lexer(text);
        SyntaxToken token;
        var tokens = new List<SyntaxToken>();
        do
        {
            token = lexer.NextToken();
            if (token.Kind != SyntaxKind.WhitespaceToken &&
                token.Kind != SyntaxKind.BadToken)
            {
                tokens.Add(token);
            }
        } while (token.Kind != SyntaxKind.EndOfFileToken);

        this.tokens = tokens.ToArray();
        this.diagnostics.AddRange(lexer.Diagnostics);
    }

    private SyntaxToken Peek(int offset)
    {
        var index = this.position + offset;
        if (index >= this.tokens.Length)
        {
            return this.tokens[this.tokens.Length - 1];
        }

        return this.tokens[index];
    }

    private SyntaxToken Current => this.Peek(0);

    private SyntaxToken NextToken()
    {
        var current = this.Current;
        this.position++;
        return current;
    }

    private SyntaxToken Match(SyntaxKind kind)
    {
        if (this.Current.Kind == kind)
        {
            return this.NextToken();
        }

        this.diagnostics.Add($"error: Unexpected token <{this.Current.Kind}> expected <{kind}>");
        return new SyntaxToken(kind, this.Current.Position, null, null);
    }

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        var unaryOperatorPrecedence = this.Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            var unaryOperator = this.NextToken();
            var operand = this.ParseExpression(unaryOperatorPrecedence);
            left = new UnaryExpressionSyntax(unaryOperator, operand);
        }
        else
        {
            left = this.ParsePrimaryExpression();
        }

        while (true)
        {
            var precedence = this.Current.Kind.GetBinaryOperatorPrecedence();
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }

            var operatorToken = this.NextToken();
            var right = this.ParseExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    public SyntaxTree Parse()
    {
        var expression = this.ParseExpression();
        var endOfFileToken = this.Match(SyntaxKind.EndOfFileToken);
        return new SyntaxTree(this.diagnostics, expression, endOfFileToken);
    }


    private ExpressionSyntax ParsePrimaryExpression()
    {
        if (this.Current.Kind == SyntaxKind.OpenParenthesisToken)
        {
            var left = this.NextToken();
            var expression = this.ParseExpression();
            var right = this.Match(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(
                left,
                expression,
                right);
        }

        if (Current.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
        {
            var value = Current.Kind == SyntaxKind.TrueKeyword;
            var token = this.NextToken();
            return new LiteralExpressionSyntax(token, value);
        }

        var numberToken = this.Match(SyntaxKind.NumberToken);
        return new LiteralExpressionSyntax(numberToken);
    }
}