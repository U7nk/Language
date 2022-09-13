using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class Parser
{
    public int Position;
    public ImmutableArray<SyntaxToken> Tokens;
    readonly DiagnosticBag _diagnostic = new();
    public IEnumerable<Diagnostic> Diagnostic => _diagnostic;

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

        this.Tokens = tokens.ToImmutableArray();
        _diagnostic.AddRange(lexer.Diagnostics);
    }

    SyntaxToken Peek(int offset)
    {
        var index = Position + offset;
        if (index >= Tokens.Length)
        {
            return Tokens.Last();
        }

        return Tokens[index];
    }

    SyntaxToken Current => Peek(0);

    SyntaxToken NextToken()
    {
        var current = Current;
        Position++;

        return current;
    }

    SyntaxToken Match(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        _diagnostic.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
        return new SyntaxToken(kind, Current.Position, string.Empty, null);
    }

    ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            var unaryOperator = NextToken();
            var operand = ParseBinaryExpression(unaryOperatorPrecedence);
            left = new UnaryExpressionSyntax(unaryOperator, operand);
        }
        else
        {
            left = ParsePrimaryExpression();
        }

        while (true)
        {
            var precedence = Current.Kind.GetBinaryOperatorPrecedence();
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }

            var operatorToken = NextToken();
            var right = ParseBinaryExpression(precedence);
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }


    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var statement = ParseStatement();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new CompilationUnitSyntax(statement, endOfFileToken);
    }

    StatementSyntax ParseStatement()
    {
        if (Current.Kind is SyntaxKind.OpenBraceToken)
            return ParseBlockStatement();

        if (Current.Kind is SyntaxKind.LetKeyword or SyntaxKind.VarKeyword)
            return ParseVariableDeclarationStatement();

        if (Current.Kind is SyntaxKind.IfKeyword)
            return ParseIfStatement();

        if (Current.Kind is SyntaxKind.WhileKeyword)
            return ParseWhileStatement();
        
        if (Current.Kind is SyntaxKind.ForKeyword)
            return ParseForStatement();

        return ParseExpressionStatement();
    }

    StatementSyntax ParseWhileStatement()
    {
        var whileKeyword = Match(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var body = ParseStatement();
        return new WhileStatementSyntax(whileKeyword, condition, body);
    }

    StatementSyntax ParseIfStatement()
    {
        var ifKeyword = Match(SyntaxKind.IfKeyword);
        var condition = ParseExpression();
        var thenStatement = ParseStatement();
        var elseClause = ParseElseClause();
        return new IfStatementSyntax(ifKeyword, condition, thenStatement, elseClause);
    }

    ForStatementSyntax ParseForStatement()
    {
        var forKeyword = Match(SyntaxKind.ForKeyword);
        var openParenthesis = Match(SyntaxKind.OpenParenthesisToken);
        
        VariableDeclarationAssignmentSyntax? variableDeclaration = null;
        ExpressionSyntax? expression = null;
        if (Current.Kind is SyntaxKind.VarKeyword)
            variableDeclaration = ParseVariableDeclarationAssignmentSyntax();
        else
            expression = ParseExpression();
        
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        var condition = ParseExpression();
        var middleSemicolonToken = Match(SyntaxKind.SemicolonToken);
        var mutation = ParseExpression();
        var closeParenthesis = Match(SyntaxKind.CloseParenthesisToken);
        var body = ParseStatement();

        return new ForStatementSyntax(
            forKeyword, openParenthesis, 
            variableDeclaration, expression,
            semicolonToken, condition,
            middleSemicolonToken, mutation,
            closeParenthesis, body);
    }


    ElseClauseSyntax? ParseElseClause()
    {
        if (Current.Kind is not SyntaxKind.ElseKeyword)
            return null;

        var elseKeyword = NextToken();
        var elseStatement = ParseStatement();
        return new ElseClauseSyntax(elseKeyword, elseStatement);
    }

    VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var variableDeclarationAssignment = ParseVariableDeclarationAssignmentSyntax();
        var semicolon = Match(SyntaxKind.SemicolonToken);
        
        return new VariableDeclarationStatementSyntax(variableDeclarationAssignment, semicolon);
    }


    VariableDeclarationAssignmentSyntax ParseVariableDeclarationAssignmentSyntax()
    {
        var variableDeclaration = ParseVariableDeclarationSyntax();
        var equals = Match(SyntaxKind.EqualsToken);
        var initializer = ParseExpression();
        return new VariableDeclarationAssignmentSyntax(variableDeclaration, equals, initializer);
    }

    VariableDeclarationSyntax ParseVariableDeclarationSyntax()
    {
        var keyword = Match(
            Current.Kind is SyntaxKind.VarKeyword
                ? SyntaxKind.VarKeyword
                : SyntaxKind.LetKeyword);

        var identifier = Match(SyntaxKind.IdentifierToken);
        var typeClause = ParseTypeClause();
        return new(keyword, identifier, typeClause);
    }

    TypeClauseSyntax? ParseTypeClause()
    {
        if (Current.Kind is not SyntaxKind.ColonToken)
            return null;
        
        var colon = NextToken();
        var type = Match(SyntaxKind.IdentifierToken);
        return new(colon, type);
    }

    StatementSyntax ParseBlockStatement()
    {
        var openBraceToken = Match(SyntaxKind.OpenBraceToken);
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        while (Current.Kind
               is not SyntaxKind.CloseBraceToken
               and not SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var statement = ParseStatement();
            statements.Add(statement);

            // if ParseStatement() did not consume any tokens, we're in an infinite loop
            // so we need to consume at least one token to prevent looping
            //
            // no need for error reporting, because ParseStatement() already reported it
            if (ReferenceEquals(Current, startToken)) 
                NextToken();
        }

        var closeBraceToken = Match(SyntaxKind.CloseBraceToken);
        return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        return new ExpressionStatementSyntax(expression, semicolonToken);
    }

    ExpressionSyntax ParseExpression()
        => ParseAssignmentExpression();

    ExpressionSyntax ParseAssignmentExpression()
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

        if (Current.Kind is SyntaxKind.IdentifierToken
            && Peek(1).Kind is SyntaxKind.EqualsToken)
        {
            var identifier = Match(SyntaxKind.IdentifierToken);
            var equalsToken = Match(SyntaxKind.EqualsToken);
            var right = ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(identifier, equalsToken, right);
        }

        return ParseBinaryExpression();
    }

    ExpressionSyntax ParsePrimaryExpression()
    {
        return Current.Kind switch
        {
            SyntaxKind.OpenParenthesisToken =>
                ParseParenthesizedExpression(),
            SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword =>
                ParseBooleanLiteralExpression(),
            SyntaxKind.NumberToken =>
                ParseNumberLiteralExpression(),
            SyntaxKind.StringToken =>
                ParseStringLiteralExpression(),
            _ /*default*/ =>
                ParseNameOrCallExpression()
        };
    }

    ExpressionSyntax ParseStringLiteralExpression()
    {
        var token = Match(SyntaxKind.StringToken);
        return new LiteralExpressionSyntax(token);
    }

    ExpressionSyntax ParseNameOrCallExpression()
    {
        if (Current.Kind == SyntaxKind.IdentifierToken
            && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
        {
            return ParseCallExpression();
        }

        return ParseNameExpression();
    }

    ExpressionSyntax ParseCallExpression()
    {
        var identifier = Match(SyntaxKind.IdentifierToken);
        var openParenthesis = Match(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseArguments();
        var closeParenthesis = Match(SyntaxKind.CloseParenthesisToken);
        return new CallExpressionSyntax(identifier, openParenthesis, arguments, closeParenthesis);
    }

    SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
    {
        if (Current.Kind is SyntaxKind.CloseParenthesisToken)
            return new(ImmutableArray<SyntaxNode>.Empty);
        
        var arguments = ImmutableArray.CreateBuilder<SyntaxNode>();
        arguments.Add(ParseExpression());
        while (Current.Kind is SyntaxKind.CommaToken)
        {
            var comma = Match(SyntaxKind.CommaToken);
            arguments.Add(comma);
            arguments.Add(ParseExpression());
        }
        
        return new(arguments.ToImmutable());
    }

    ExpressionSyntax ParseNumberLiteralExpression()
    {
        var numberToken = Match(SyntaxKind.NumberToken);
        return new LiteralExpressionSyntax(numberToken);
    }

    ExpressionSyntax ParseParenthesizedExpression()
    {
        var left = Match(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        var right = Match(SyntaxKind.CloseParenthesisToken);
        return new ParenthesizedExpressionSyntax(
            left,
            expression,
            right);
    }

    ExpressionSyntax ParseBooleanLiteralExpression()
    {
        var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
        var token = Match(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new LiteralExpressionSyntax(token, isTrue);
    }

    ExpressionSyntax ParseNameExpression()
    {
        var token = Match(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(token);
    }
}