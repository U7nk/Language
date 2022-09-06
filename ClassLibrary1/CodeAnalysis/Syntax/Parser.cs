using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class Parser
{
    public int position;
    public ImmutableArray<SyntaxToken> tokens;
    private readonly DiagnosticBag diagnostic = new();
    public IEnumerable<Diagnostic> Diagnostic => this.diagnostic;

    public Parser(SourceText text)
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

        this.tokens = tokens.ToImmutableArray();
        this.diagnostic.AddRange(lexer.Diagnostics);
    }

    private SyntaxToken Peek(int offset)
    {
        var index = this.position + offset;
        if (index >= this.tokens.Length)
        {
            return this.tokens.Last();
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


    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var statement = this.ParseStatement();
        var endOfFileToken = this.Match(SyntaxKind.EndOfFileToken);
        return new CompilationUnitSyntax(statement, endOfFileToken);
    }

    private StatementSyntax ParseStatement()
    {
        if (this.Current.Kind is SyntaxKind.OpenBraceToken)
            return ParseBlockStatement();

        if (this.Current.Kind is SyntaxKind.LetKeyword or SyntaxKind.VarKeyword)
            return this.ParseVariableDeclarationStatement();

        return this.ParseExpressionStatement();
    }

    private VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var keyword = this.Match(
            this.Current.Kind is SyntaxKind.VarKeyword
                ? SyntaxKind.VarKeyword
                : SyntaxKind.LetKeyword);
        
        var identifier = this.Match(SyntaxKind.IdentifierToken);
        var equals = this.Match(SyntaxKind.EqualsToken);
        var initializer = this.ParseExpression();
        var semicolon = this.Match(SyntaxKind.SemicolonToken);
        return new VariableDeclarationStatementSyntax(keyword, identifier, equals, initializer, semicolon);
    }

    private StatementSyntax ParseBlockStatement()
    {
        var openBraceToken = this.Match(SyntaxKind.OpenBraceToken);
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        while (this.Current.Kind
               is not SyntaxKind.CloseBraceToken
               and not SyntaxKind.EndOfFileToken)
        {
            var statement = this.ParseStatement();
            statements.Add(statement);
        }

        var closeBraceToken = this.Match(SyntaxKind.CloseBraceToken);
        return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    private ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = this.ParseExpression();
        var semicolonToken = this.Match(SyntaxKind.SemicolonToken);
        return new ExpressionStatementSyntax(expression, semicolonToken);
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
        return this.Current.Kind switch
        {
            SyntaxKind.OpenParenthesisToken =>
                this.ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword =>
                this.ParseBooleanLiteralExpression(),
            SyntaxKind.IdentifierToken =>
                this.ParseNameExpression(),
            _ /*default*/ =>
                this.ParseNumberLiteralExpression()
        };
    }

    private ExpressionSyntax ParseNumberLiteralExpression()
    {
        var numberToken = this.Match(SyntaxKind.NumberToken);
        return new LiteralExpressionSyntax(numberToken);
    }

    private ExpressionSyntax ParseParenthesizedExpression()
    {
        var left = this.Match(SyntaxKind.OpenParenthesisToken);
        var expression = this.ParseExpression();
        var right = this.Match(SyntaxKind.CloseParenthesisToken);
        return new ParenthesizedExpressionSyntax(
            left,
            expression,
            right);
    }

    private ExpressionSyntax ParseBooleanLiteralExpression()
    {
        var isTrue = this.Current.Kind == SyntaxKind.TrueKeyword;
        var token = this.Match(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new LiteralExpressionSyntax(token, isTrue);
    }

    private ExpressionSyntax ParseNameExpression()
    {
        var token = this.Match(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(token);
    }
}