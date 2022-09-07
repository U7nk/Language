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

        if (this.Current.Kind is SyntaxKind.IfKeyword)
            return this.ParseIfStatement();

        if (this.Current.Kind is SyntaxKind.WhileKeyword)
            return this.ParseWhileStatement();
        
        if (this.Current.Kind is SyntaxKind.ForKeyword)
            return this.ParseForStatement();

        return this.ParseExpressionStatement();
    }

    private StatementSyntax ParseWhileStatement()
    {
        var whileKeyword = this.Match(SyntaxKind.WhileKeyword);
        var condition = this.ParseExpression();
        var body = this.ParseStatement();
        return new WhileStatementSyntax(whileKeyword, condition, body);
    }

    private StatementSyntax ParseIfStatement()
    {
        var ifKeyword = this.Match(SyntaxKind.IfKeyword);
        var condition = this.ParseExpression();
        var thenStatement = this.ParseStatement();
        var elseClause = this.ParseElseClause();
        return new IfStatementSyntax(ifKeyword, condition, thenStatement, elseClause);
    }

    private ForStatementSyntax ParseForStatement()
    {
        var forKeyword = this.Match(SyntaxKind.ForKeyword);
        var openParenthesis = this.Match(SyntaxKind.OpenParenthesisToken);
        
        VariableDeclarationAssignmentSyntax? variableDeclaration = null;
        ExpressionSyntax? expression = null;
        if (this.Current.Kind is SyntaxKind.VarKeyword)
            variableDeclaration = this.ParseVariableDeclarationAssignmentSyntax();
        else
            expression = this.ParseExpression();
        
        var semicolonToken = this.Match(SyntaxKind.SemicolonToken);
        var condition = this.ParseExpression();
        var middleSemicolonToken = this.Match(SyntaxKind.SemicolonToken);
        var mutation = this.ParseExpression();
        var closeParenthesis = this.Match(SyntaxKind.CloseParenthesisToken);
        var body = this.ParseStatement();

        return new ForStatementSyntax(
            forKeyword, openParenthesis, 
            variableDeclaration, expression,
            semicolonToken, condition,
            middleSemicolonToken, mutation,
            closeParenthesis, body);
    }
    

    private ElseClauseSyntax? ParseElseClause()
    {
        if (this.Current.Kind is not SyntaxKind.ElseKeyword)
            return null;

        var elseKeyword = this.NextToken();
        var elseStatement = this.ParseStatement();
        return new ElseClauseSyntax(elseKeyword, elseStatement);
    }

    private VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var variableDeclarationAssignment = this.ParseVariableDeclarationAssignmentSyntax();
        var semicolon = this.Match(SyntaxKind.SemicolonToken);
        
        return new VariableDeclarationStatementSyntax(variableDeclarationAssignment, semicolon);
    }


    private VariableDeclarationAssignmentSyntax ParseVariableDeclarationAssignmentSyntax()
    {
        var variableDeclaration = this.ParseVariableDeclarationSyntax();
        var equals = this.Match(SyntaxKind.EqualsToken);
        var initializer = this.ParseExpression();
        return new VariableDeclarationAssignmentSyntax(variableDeclaration, equals, initializer);
    }
    private VariableDeclarationSyntax ParseVariableDeclarationSyntax()
    {
        var keyword = this.Match(
            this.Current.Kind is SyntaxKind.VarKeyword
                ? SyntaxKind.VarKeyword
                : SyntaxKind.LetKeyword);

        var identifier = this.Match(SyntaxKind.IdentifierToken);
        return new VariableDeclarationSyntax(keyword, identifier);
    }

    private StatementSyntax ParseBlockStatement()
    {
        var openBraceToken = this.Match(SyntaxKind.OpenBraceToken);
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        while (this.Current.Kind
               is not SyntaxKind.CloseBraceToken
               and not SyntaxKind.EndOfFileToken)
        {
            var startToken = this.Current;
            var statement = this.ParseStatement();
            statements.Add(statement);

            // if ParseStatement() did not consume any tokens, we're in an infinite loop
            // so we need to consume at least one token to prevent looping
            //
            // no need for error reporting, because ParseStatement() already reported it
            if (ReferenceEquals(this.Current, startToken)) 
                this.NextToken();
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
            SyntaxKind.NumberToken =>
                this.ParseNumberLiteralExpression(),
            _ /*default*/ =>
                this.ParseNameExpression()
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