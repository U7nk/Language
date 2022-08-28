using System;
using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public class Parser
{
    public int position;
    public SyntaxToken[] tokens;
    private readonly DiagnosticBag diagnostic = new();
    public IEnumerable<Diagnostic> Diagnostic => this.diagnostic;

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
        this.diagnostic.AddRange(lexer.Diagnostics);
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

        this.diagnostic.ReportUnexpectedToken(this.Current.Span, this.Current.Kind, kind);
        return new SyntaxToken(kind, this.Current.Position, string.Empty, null);
    }

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        var unaryOperatorPrecedence = this.Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            var unaryOperator = this.NextToken();
            var operand = this.ParseBinaryExpression(unaryOperatorPrecedence);
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
            var right = this.ParseBinaryExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }
    
    

    public SyntaxTree Parse()
    {
        var expression = this.ParseExpression();
        var endOfFileToken = this.Match(SyntaxKind.EndOfFileToken);
        return new SyntaxTree(this.diagnostic, expression, endOfFileToken);
    }

    private ExpressionSyntax ParseExpression() 
        => this.ParseAssignmentExpression();

    private ExpressionSyntax ParseAssignmentExpression()
    {
        // a + b + 5
        // is left associative
        //      +
        //     / \
        //    +   5
        //   / \
        //  a   b
        //
        // a = b = 5
        // is right associative
        //      =
        //     / \
        //    a   =
        //       / \
        //      b   5
        
        if (this.Current.Kind is SyntaxKind.IdentifierToken
            && this.Peek(1).Kind is SyntaxKind.EqualsToken)
        {
            
            var identifier = this.Match(SyntaxKind.IdentifierToken);
            var equalsToken = this.Match(SyntaxKind.EqualsToken);
            var right = this.ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(identifier, equalsToken, right);
        }

        return this.ParseBinaryExpression();
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
        
        if (Current.Kind is SyntaxKind.IdentifierToken)
        {
            var token = this.NextToken();
            return new NameExpressionSyntax(token);
        }

        var numberToken = this.Match(SyntaxKind.NumberToken);
        return new LiteralExpressionSyntax(numberToken);
    }
}