using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Syntax;

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
        SyntaxToken token = null!;
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

    Option<SyntaxToken> OptionalMatch(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        return null;
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
        var members = ParseTopMembers();
        var endOfFileToken = Match(SyntaxKind.EndOfFileToken);
        return new CompilationUnitSyntax(_syntaxTree, members, endOfFileToken);
    }

    ImmutableArray<ITopMemberDeclarationSyntax> ParseTopMembers()
    {
        var topMembers = ImmutableArray.CreateBuilder<ITopMemberDeclarationSyntax>();
        while (Current.Kind is not SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var topMember = ParseTopMember();
            topMembers.Add(topMember);

            // if ParseStatement() did not consume any tokens, we're in an infinite loop
            // so we need to consume at least one token to prevent looping
            //
            // no need for error reporting, because ParseStatement() already reported it
            if (ReferenceEquals(Current, startToken))
                NextToken();
        }

        return topMembers.ToImmutable();
    }

    ITopMemberDeclarationSyntax ParseTopMember()
    {
        if (Current.Kind == SyntaxKind.ClassKeyword)
            return ParseClassDeclaration();
        
        if (Current.Kind == SyntaxKind.FunctionKeyword)
            return ParseMethodDeclaration();
        
        return ParseGlobalStatement();
    }

    ClassDeclarationSyntax ParseClassDeclaration()
    {
        var classKeyword = Match(SyntaxKind.ClassKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var inheritanceClause = ParseOptionalInheritanceClause();
        var openBraceToken = Match(SyntaxKind.OpenBraceToken);
        var members = ParseClassMembers();
        var closeBraceToken = Match(SyntaxKind.CloseBraceToken);
        
        return new ClassDeclarationSyntax(
                    _syntaxTree, classKeyword,
                    identifier, inheritanceClause,
                    openBraceToken, 
                    members,
                    closeBraceToken);
    }

    InheritanceClauseSyntax? ParseOptionalInheritanceClause()
    {
        var colonToken = OptionalMatch(SyntaxKind.ColonToken);
        if (colonToken.IsNone)
            return null;
        
        var baseTypes = ImmutableArray.CreateBuilder<SyntaxToken>();
        Option<SyntaxToken> currentToken = Match(SyntaxKind.IdentifierToken);
        baseTypes.Add(currentToken.Unwrap());
        currentToken = OptionalMatch(SyntaxKind.CommaToken);
        if (currentToken.IsSome)
        {
            baseTypes.Add(currentToken.Unwrap());
        }
        
        while (currentToken.IsSome && currentToken.Unwrap() is { Kind: SyntaxKind.CommaToken })
        {
            var baseTypeIdentifier = Match(SyntaxKind.IdentifierToken);
            baseTypes.Add(baseTypeIdentifier);
            currentToken = OptionalMatch(SyntaxKind.CommaToken);
            if (currentToken.IsSome) 
                baseTypes.Add(currentToken.Unwrap());
        }
        
        
        var baseTypeList = new SeparatedSyntaxList<SyntaxToken>(baseTypes.ToImmutable());
        return new InheritanceClauseSyntax(_syntaxTree, baseTypeList);
    }

    ImmutableArray<IClassMemberDeclarationSyntax> ParseClassMembers()
    {
        var members = ImmutableArray.CreateBuilder<IClassMemberDeclarationSyntax>();
        while (Current.Kind is SyntaxKind.FunctionKeyword or SyntaxKind.IdentifierToken or SyntaxKind.StaticKeyword)
        {
            switch (Current.Kind)
            {
                case SyntaxKind.StaticKeyword:
                    switch (Peek(1).Kind)
                    {
                        case SyntaxKind.FunctionKeyword:
                            members.Add(ParseMethodDeclaration());
                            break;
                        case SyntaxKind.IdentifierToken:
                            members.Add(ParseFieldDeclaration());
                            break;
                    }
                    break;
                case SyntaxKind.FunctionKeyword:
                    members.Add(ParseMethodDeclaration());
                    continue;
                case SyntaxKind.IdentifierToken:
                    members.Add(ParseFieldDeclaration());
                    continue;
                default:
                    throw new Exception("Unexpected token");
            }
        }

        return members.ToImmutable();
    }

    FieldDeclarationSyntax ParseFieldDeclaration()
    {
        var staticKeyword = OptionalMatch(SyntaxKind.StaticKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var typeClause = ParseTypeClause();
        var semicolonToken = Match(SyntaxKind.SemicolonToken);
        return _syntaxTree.NewFieldDeclaration(staticKeyword, identifier, typeClause, semicolonToken);
    }
    MethodDeclarationSyntax ParseMethodDeclaration()
    {
        var staticKeyword = OptionalMatch(SyntaxKind.StaticKeyword);
        var functionKeyword = Match(SyntaxKind.FunctionKeyword);
        var virtualKeyword = OptionalMatch(SyntaxKind.VirtualKeyword);
        var overrideKeyword = OptionalMatch(SyntaxKind.OverrideKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var genericArgumentsSyntax = ParseOptionalGenericClause();
        var openParenthesisToken = Match(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameterList();
        var closeParenthesisToken = Match(SyntaxKind.CloseParenthesisToken);
        var type = ParseOptionalTypeClause();
        var body = ParseBlockStatement();
        return _syntaxTree.NewMethodDeclaration(staticKeyword, functionKeyword,
                                                virtualKeyword, overrideKeyword,
                                                identifier, genericArgumentsSyntax,
                                                openParenthesisToken,
                                                parameters, closeParenthesisToken,
                                                type, body);
    }

    Option<GenericClauseSyntax> ParseOptionalGenericClause()
    {
        var lessThanToken = OptionalMatch(SyntaxKind.LessThanToken);
        if (lessThanToken.IsNone)
            return Option.None;

        var arguments = ImmutableArray.CreateBuilder<SyntaxToken>();
        var currentToken = Match(SyntaxKind.IdentifierToken);
        arguments.Add(currentToken);
        
        while (Current is { Kind: SyntaxKind.CommaToken })
        {
            Match(SyntaxKind.CommaToken).AddTo(arguments);
            Match(SyntaxKind.IdentifierToken).AddTo(arguments);
        }
        
        var greaterThanToken = Match(SyntaxKind.GreaterThanToken);
        return _syntaxTree.NewGenericClause(lessThanToken.Unwrap(),
                                                   new SeparatedSyntaxList<SyntaxToken>(arguments.ToImmutable()),
                                                   greaterThanToken);
    }

    GlobalStatementDeclarationSyntax ParseGlobalStatement()
    {
        var statement = ParseStatement();
        return new GlobalStatementDeclarationSyntax(_syntaxTree, statement);
    }

    SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
    {
        if (Current.Kind is SyntaxKind.CloseParenthesisToken)
            return new SeparatedSyntaxList<ParameterSyntax>(ImmutableArray<ParameterSyntax>.Empty);

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

    StatementSyntax ParseVariableDeclarationStatement()
    {
        var variableDeclaration = ParseVariableDeclarationSyntax();
        if (Current.Kind is SyntaxKind.EqualsToken)
        {
            var equals = Match(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            var variableDeclarationAssignment =
                _syntaxTree.NewVariableDeclarationAssignment(variableDeclaration, equals, initializer); 
            var semicolon = Match(SyntaxKind.SemicolonToken);
            return _syntaxTree.NewVariableDeclarationAssignmentStatement(variableDeclarationAssignment, semicolon);    
        }
        else
        {
            var semicolon = Match(SyntaxKind.SemicolonToken);
            return new VariableDeclarationStatementSyntax(_syntaxTree, variableDeclaration, semicolon);
        }
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
        return _syntaxTree.NewTypeClause(colon, type);
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
        return _syntaxTree.NewExpressionStatement(expression, semicolonToken);
    }

    ExpressionSyntax ParseExpression()
    {
        if (Current.Kind is SyntaxKind.NewKeyword)
        {
            return ParseObjectCreationExpression();
        }
        
        var binary =  ParseBinaryExpression();

        return binary;
    }

    MethodCallExpressionSyntax ParseMethodCallExpressionSyntax()
    {
        var identifier = Match(SyntaxKind.IdentifierToken);
        var optionalGenericArguments = ParseOptionalGenericClause();
        var openParenthesis = Match(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseArguments();
        var closeParenthesis = Match(SyntaxKind.CloseParenthesisToken);
        return _syntaxTree.NewMethodCallExpression(identifier, optionalGenericArguments, openParenthesis, arguments, closeParenthesis);
    }
    
    ExpressionSyntax ParseMemberAccessExpression()
    {
        var left = Peek(1).Kind switch
        {
            SyntaxKind.OpenParenthesisToken => ParseMethodCallExpressionSyntax(),
            SyntaxKind.LessThanToken when Peek(1).Kind is SyntaxKind.OpenParenthesisToken => ParseMethodCallExpressionSyntax(),

            _ => Current.Kind is SyntaxKind.ThisKeyword 
                ? ParseThisExpression() 
                : ParseNameExpression()
        };


        while (Current.Kind is SyntaxKind.DotToken)
        {
            var dot = Match(SyntaxKind.DotToken);
            
            ExpressionSyntax right = Peek(1).Kind switch
            {
                SyntaxKind.OpenParenthesisToken => ParseMethodCallExpressionSyntax(),
                SyntaxKind.LessThanToken => ParseMethodCallExpressionSyntax(),
                
                _ => ParseNameExpression()
            };
            left = _syntaxTree.NewMemberAccessExpression(left, dot, right);
        }

        return left;
    }

    ObjectCreationExpressionSyntax ParseObjectCreationExpression()
    {
        var newKeyword = Match(SyntaxKind.NewKeyword);
        var identifier = Match(SyntaxKind.IdentifierToken);
        var openParenthesis = Match(SyntaxKind.OpenParenthesisToken);
        var closeParenthesis = Match(SyntaxKind.CloseParenthesisToken);
        
        return _syntaxTree.NewObjectCreationExpression(
            newKeyword, identifier,
            openParenthesis, closeParenthesis);
    }

    AssignmentExpressionSyntax ParseAssignmentExpression()
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
        // assignment is right associative
        //      =
        //     / \
        //    a   =
        //       / \
        //      b   5

        var identifier = Match(SyntaxKind.IdentifierToken);
        var equalsToken = Match(SyntaxKind.EqualsToken);
        var right = ParseExpression();
        return _syntaxTree.NewAssignmentExpression(identifier, equalsToken, right);
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
                ParseMemberAccessOrAssignment()
        };
    }

    ExpressionSyntax ParseStringLiteralExpression()
    {
        var token = Match(SyntaxKind.StringToken);
        return new LiteralExpressionSyntax(_syntaxTree,token);
    }

    ExpressionSyntax ParseNameExpression()
    {
        var token = Match(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(_syntaxTree, token);
    }

    ExpressionSyntax ParseThisExpression()
    {
        var thisToken = Match(SyntaxKind.ThisKeyword);
        return new ThisExpressionSyntax(_syntaxTree, thisToken); 
    }
    
    ExpressionSyntax ParseMemberAccessOrAssignment()
    {
        var memberAccess = ParseMemberAccessExpression();
        if (Current.Kind is SyntaxKind.EqualsToken)
        {
            var equals = Match(SyntaxKind.EqualsToken);
            var right = ParseExpression();
            return _syntaxTree.NewMemberAssignmentExpression(memberAccess, equals, right);
        }

        return memberAccess;
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
        var openParenthesisToken = Match(SyntaxKind.OpenParenthesisToken);
        var nameExpression = ParseExpression();
        var closeParenthesisToken = Match(SyntaxKind.CloseParenthesisToken);
        if (Current.Kind is SyntaxKind.DotToken or SyntaxKind.SemicolonToken or SyntaxKind.OpenBraceToken
            || Current.Kind.GetBinaryOperatorPrecedence() > 0)
        {
            return _syntaxTree.NewParenthesizedExpression(openParenthesisToken, nameExpression, closeParenthesisToken);
        }
        
        var castedExpression = ParseExpression();
        if (nameExpression.Kind is not SyntaxKind.NameExpression)
        {
            _diagnostics.ReportUnexpectedExpressionToCast(nameExpression);
            return castedExpression;
        }
        
        return _syntaxTree.NewCastExpression(openParenthesisToken,
                                             (NameExpressionSyntax)nameExpression, 
                                             closeParenthesisToken,
                                             castedExpression);
    }

    ExpressionSyntax ParseBooleanLiteralExpression()
    {
        var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
        var token = Match(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);
        return new LiteralExpressionSyntax(_syntaxTree, token, isTrue);
    }
    
}