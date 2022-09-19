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
    int _position;
    readonly ImmutableArray<SyntaxToken> _tokens;
    readonly DiagnosticBag _diagnostics = new();
    readonly SyntaxTree _syntaxTree;
    readonly SourceText _sourceText;
    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    public Parser(SyntaxTree syntaxTree)
    {
        var lexer = new Lexer(syntaxTree);
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

        _syntaxTree = syntaxTree;
        _sourceText = syntaxTree.SourceText;
        _tokens = tokens.ToImmutableArray();
        _diagnostics.AddRange(lexer.Diagnostics);
    }

    SyntaxToken Peek(int offset)
    {
        var index = _position + offset;
        if (index >= _tokens.Length)
        {
            return _tokens.Last();
        }

        return _tokens[index];
    }

    SyntaxToken Current => Peek(0);

    SyntaxToken NextToken()
    {
        var current = Current;
        _position++;

        return current;
    }

    SyntaxToken Match(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        _diagnostics.ReportUnexpectedToken(
            new TextLocation(_sourceText, Current.Span),
            Current.Kind, kind);
        return new SyntaxToken(_syntaxTree, kind, Current.Position, string.Empty, null);
    }

    ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            var unaryOperator = NextToken();
            var operand = ParseBinaryExpression(unaryOperatorPrecedence);
            left = new UnaryExpressionSyntax(_syntaxTree, unaryOperator, operand);
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
            left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
        }

        return left;
    }


    public CompilationUnitSyntax ParseCompilationUnit()
    {
        var members = ParseMembers();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new CompilationUnitSyntax(_syntaxTree, members, endOfFileToken);
    }

    ImmutableArray<MemberSyntax> ParseMembers()
    {
        var members = ImmutableArray.CreateBuilder<MemberSyntax>();
        while (Current.Kind is not SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var member = ParseMember();
            members.Add(member);

            // if ParseStatement() did not consume any tokens, we're in an infinite loop
            // so we need to consume at least one token to prevent looping
            //
            // no need for error reporting, because ParseStatement() already reported it
            if (ReferenceEquals(Current, startToken))
                NextToken();
        }

        return members.ToImmutable();
    }

    MemberSyntax ParseMember()
    {
        if (Current.Kind == SyntaxKind.FunctionKeyword)
            return ParseFunctionDeclaration();

        return ParseGlobalStatement();
    }

    GlobalStatementSyntax ParseGlobalStatement()
    {
        var statement = ParseStatement();
        return new GlobalStatementSyntax(_syntaxTree, statement);
    }

    MemberSyntax ParseFunctionDeclaration()
    {
        var functionKeyword = Match(SyntaxKind.FunctionKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var openParenthesisToken = Match(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameterList();
        var closeParenthesisToken = Match(SyntaxKind.CloseParenthesisToken);
        var type = ParseOptionalTypeClause();
        var body = ParseBlockStatement();
        return new FunctionDeclarationSyntax(_syntaxTree, functionKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, type, body);
    }

    SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
    {
        if (Current.Kind is SyntaxKind.CloseParenthesisToken)
            return new SeparatedSyntaxList<ParameterSyntax>(ImmutableArray<SyntaxNode>.Empty);

        var parameters = ImmutableArray.CreateBuilder<SyntaxNode>();
        while (true)
        {
            var parameter = ParseParameter();
            parameters.Add(parameter);

            if (Current.Kind is not SyntaxKind.CommaToken)
                break;

            var comma = Match(SyntaxKind.CommaToken);
            parameters.Add(comma);
        }

        return new SeparatedSyntaxList<ParameterSyntax>(parameters.ToImmutable());
    }

    ParameterSyntax ParseParameter()
    {
        var identifier = Match(SyntaxKind.IdentifierToken);
        var type = ParseTypeClause();
        return new ParameterSyntax(_syntaxTree, identifier, type);
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

        if (Current.Kind is SyntaxKind.BreakKeyword)
            return ParseBreakStatement();

        if (Current.Kind is SyntaxKind.ContinueKeyword)
            return ParseContinueStatement();
        
        if (Current.Kind is SyntaxKind.ReturnKeyword)
            return ParseReturnStatement();

        return ParseExpressionStatement();
    }

    StatementSyntax ParseReturnStatement()
    {
        var keyword = Match(SyntaxKind.ReturnKeyword);
        var expression = Current.Kind is SyntaxKind.SemicolonToken 
            ? null
            : ParseExpression();
        
        var semicolon = Match(SyntaxKind.SemicolonToken);
        return new ReturnStatementSyntax(_syntaxTree, keyword, expression, semicolon);
    }

    StatementSyntax ParseContinueStatement()
    {
        var continueKeyword = Match(SyntaxKind.ContinueKeyword);
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        return new ContinueStatementSyntax(_syntaxTree, continueKeyword, semicolonToken);
    }

    StatementSyntax ParseBreakStatement()
    {
        var breakKeyword = Match(SyntaxKind.BreakKeyword);
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        return new BreakStatementSyntax(_syntaxTree, breakKeyword, semicolonToken);
    }

    StatementSyntax ParseWhileStatement()
    {
        var whileKeyword = Match(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var body = ParseStatement();
        return new WhileStatementSyntax(_syntaxTree, whileKeyword, condition, body);
    }

    StatementSyntax ParseIfStatement()
    {
        var ifKeyword = Match(SyntaxKind.IfKeyword);
        var condition = ParseExpression();
        var thenStatement = ParseStatement();
        var elseClause = ParseElseClause();
        return new IfStatementSyntax(_syntaxTree, ifKeyword, condition, thenStatement, elseClause);
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

        return new ForStatementSyntax(_syntaxTree, forKeyword, openParenthesis,
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
        return new ElseClauseSyntax(_syntaxTree, elseKeyword, elseStatement);
    }

    VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        var variableDeclarationAssignment = ParseVariableDeclarationAssignmentSyntax();
        var semicolon = Match(SyntaxKind.SemicolonToken);

        return new VariableDeclarationStatementSyntax(_syntaxTree, variableDeclarationAssignment, semicolon);
    }


    VariableDeclarationAssignmentSyntax ParseVariableDeclarationAssignmentSyntax()
    {
        var variableDeclaration = ParseVariableDeclarationSyntax();
        var equals = Match(SyntaxKind.EqualsToken);
        var initializer = ParseExpression();
        return new VariableDeclarationAssignmentSyntax(_syntaxTree, variableDeclaration, equals, initializer);
    }

    VariableDeclarationSyntax ParseVariableDeclarationSyntax()
    {
        var keyword = Match(
            Current.Kind is SyntaxKind.VarKeyword
                ? SyntaxKind.VarKeyword
                : SyntaxKind.LetKeyword);

        var identifier = Match(SyntaxKind.IdentifierToken);
        var typeClause = ParseOptionalTypeClause();
        return new(_syntaxTree, keyword, identifier, typeClause);
    }

    TypeClauseSyntax? ParseOptionalTypeClause()
    {
        if (Current.Kind is not SyntaxKind.ColonToken)
            return null;

        var colon = NextToken();
        var type = Match(SyntaxKind.IdentifierToken);
        return new(_syntaxTree, colon, type);
    }

    TypeClauseSyntax ParseTypeClause()
    {
        var colon = Match(SyntaxKind.ColonToken);
        var type = Match(SyntaxKind.IdentifierToken);
        return new(_syntaxTree, colon, type);
    }

    BlockStatementSyntax ParseBlockStatement()
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
        return new BlockStatementSyntax(_syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        return new ExpressionStatementSyntax(_syntaxTree, expression, semicolonToken);
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
            return new AssignmentExpressionSyntax(_syntaxTree, identifier, equalsToken, right);
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
        return new LiteralExpressionSyntax(_syntaxTree,token);
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
        return new CallExpressionSyntax(_syntaxTree, identifier, openParenthesis, arguments, closeParenthesis);
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
        return new LiteralExpressionSyntax(_syntaxTree,numberToken);
    }

    ExpressionSyntax ParseParenthesizedExpression()
    {
        var left = Match(SyntaxKind.OpenParenthesisToken);
        var expression = ParseExpression();
        var right = Match(SyntaxKind.CloseParenthesisToken);
        return new ParenthesizedExpressionSyntax(_syntaxTree, left,
            expression,
            right);
    }

    ExpressionSyntax ParseBooleanLiteralExpression()
    {
        var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
        var token = Match(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new LiteralExpressionSyntax(_syntaxTree, token, isTrue);
    }

    ExpressionSyntax ParseNameExpression()
    {
        var token = Match(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(_syntaxTree, token);
    }
}