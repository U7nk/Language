using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Wired.CodeAnalysis.Lowering;
using Wired.CodeAnalysis.Syntax;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Binding;

internal sealed class Binder
{
    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    readonly DiagnosticBag _diagnostics = new();
    BoundScope _scope;
    readonly Stack<(LabelSymbol BreakLabel, LabelSymbol ContinueLabel)> _loopStack = new();
    readonly FunctionSymbol? _function;

    Binder(BoundScope parent, FunctionSymbol? function)
    {
        _scope = new BoundScope(parent);
        _function = function;
        if (_function is not null)
        {
            foreach (var p in _function.Parameters)
                _scope.TryDeclareVariable(p);
        }
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParentScope(previous);
        var binder = new Binder(parentScope, function: null);

        foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            binder.BindFunctionDeclaration(function);


        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
        {
            var s = binder.BindStatement(globalStatement.Statement);
            statements.Add(s);
        }

        var statement = new BoundBlockStatement(statements.ToImmutable());

        var functions = binder._scope.GetDeclaredFunctions();
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();
        if (previous is not null)
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, functions, variables, statement);
    }

    void BindFunctionDeclaration(FunctionDeclarationSyntax function)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameters = new HashSet<string>();
        foreach (var parameter in function.Parameters)
        {
            var parameterName = parameter.Identifier.Text;
            if (!seenParameters.Add(parameterName))
            {
                _diagnostics.ReportParameterAlreadyDeclared(parameter.Location, parameterName);
                continue;
            }

            var parameterType = BindTypeClause(parameter.Type);
            parameters.Add(new ParameterSymbol(parameterName, parameterType));
        }

        var returnType = BindTypeClause(function.Type) ?? TypeSymbol.Void;

        var functionSymbol =
            new FunctionSymbol(function.Identifier.Text, parameters.ToImmutable(), returnType, function);
        if (!_scope.TryDeclareFunction(functionSymbol))
            _diagnostics.ReportFunctionAlreadyDeclared(function.Identifier.Location, function.Identifier.Text);
    }

    static BoundScope CreateParentScope(BoundGlobalScope? previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous is not null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();

        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach (var variable in previous.Variables)
                scope.TryDeclareVariable(variable);

            foreach (var function in previous.Functions)
                scope.TryDeclareFunction(function);

            parent = scope;
        }

        return parent;
    }

    static BoundScope CreateRootScope()
    {
        var result = new BoundScope(null);
        foreach (var functionSymbol in BuiltInFunctions.GetAll())
            result.TryDeclareFunction(functionSymbol);

        return result;
    }


    public static BoundProgram BindProgram(BoundGlobalScope globalScope)
    {
        var parentScope = CreateParentScope(globalScope);
        var functionBodies
            = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = new DiagnosticBag();

        var scope = globalScope;
        while (scope is not null)
        {
            foreach (var function in scope.Functions)
            {
                var binder = new Binder(new BoundScope(null), function);
                Debug.Assert(function.Declaration != null, "function.Declaration != null");
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);
                if (!Equals(function.ReturnType, TypeSymbol.Void)
                    && !ControlFlowGraph.AllPathsReturn(loweredBody))
                {
                    diagnostics.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);
                }

                functionBodies.Add(function, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }

            scope = scope.Previous;
        }


        var boundProgram = new BoundProgram(globalScope, diagnostics.ToImmutableArray(), functionBodies.ToImmutable());
        return boundProgram;
    }

    BoundStatement BindErrorStatement()
        => new BoundExpressionStatement(new BoundErrorExpression());

    BoundStatement BindStatement(StatementSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.BlockStatement:
                return BindBlockStatement((BlockStatementSyntax)syntax);
            case SyntaxKind.ExpressionStatement:
                return BindExpressionStatement((ExpressionStatementSyntax)syntax);
            case SyntaxKind.VariableDeclarationStatement:
                return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax)syntax);
            case SyntaxKind.IfStatement:
                return BindIfStatement((IfStatementSyntax)syntax);
            case SyntaxKind.WhileStatement:
                return BindWhileStatement((WhileStatementSyntax)syntax);
            case SyntaxKind.ForStatement:
                return BindForStatement((ForStatementSyntax)syntax);
            case SyntaxKind.ContinueStatement:
                return BindContinueStatement((ContinueStatementSyntax)syntax);
            case SyntaxKind.BreakStatement:
                return BindBreakStatement((BreakStatementSyntax)syntax);
            case SyntaxKind.ReturnStatement:
                return BindReturnStatement((ReturnStatementSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
    {
        var expression = syntax.Expression is null
            ? null
            : BindExpression(syntax.Expression);

        if (_function is null)
            _diagnostics.ReportInvalidReturn(syntax.ReturnKeyword.Location);
        else
        {
            if (_function.ReturnType == TypeSymbol.Void)
            {
                if (expression is not null)
                    _diagnostics.ReportReturnStatementIsInvalidForVoidFunction(syntax.Location);
            }
            else
            {
                if (expression is null)
                    _diagnostics.ReportReturnStatementIsInvalidForNonVoidFunction(syntax.Location);
                else
                {
                    Debug.Assert(syntax.Expression != null, "syntax.Expression != null");
                    expression = BindConversion(expression, _function.ReturnType, syntax.Expression.Location);
                }
            }
        }

        return new BoundReturnStatement(expression);
    }

    BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreakOrContinue(syntax.BreakKeyword);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(_loopStack.Peek().BreakLabel);
    }

    BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreakOrContinue(syntax.ContinueKeyword);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(_loopStack.Peek().ContinueLabel);
    }

    BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        BoundVariableDeclarationStatement? variableDeclaration = null;
        BoundExpression? expression = null;
        if (syntax.VariableDeclaration is not null)
            variableDeclaration = BindVariableDeclarationAssignmentSyntax(syntax.VariableDeclaration);
        else
            expression = BindExpression(syntax.Expression.ThrowIfNull());

        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var mutation = BindExpression(syntax.Mutation);

        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

        return new BoundForStatement(variableDeclaration, expression, condition, mutation, body, breakLabel,
            continueLabel);
    }

    BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
    }

    BoundStatement BindLoopBody(StatementSyntax body, out LabelSymbol breakLabel, out LabelSymbol continueLabel)
    {
        breakLabel = new LabelSymbol("break");
        continueLabel = new LabelSymbol("continue");
        _loopStack.Push((breakLabel, continueLabel));

        var boundBody = BindStatement(body);
        return boundBody;
    }

    BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause is null
            ? null
            : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }


    BoundVariableDeclarationStatement BindVariableDeclarationAssignmentSyntax(
        VariableDeclarationAssignmentSyntax syntax)
    {
        var isReadonly = syntax.VariableDeclaration.KeywordToken.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(syntax.VariableDeclaration.TypeClause);
        var initializer = BindExpression(syntax.Initializer);

        if (type is not null)
            initializer = BindConversion(initializer, type, syntax.Initializer.Location);

        var name = syntax.VariableDeclaration.IdentifierToken.Text;
        var variable = new VariableSymbol(name, type ?? initializer.Type, isReadonly);

        if (!_scope.TryDeclareVariable(variable))
        {
            var location = new TextLocation(syntax.SyntaxTree.SourceText,
                TextSpan.FromBounds(
                    syntax.VariableDeclaration.KeywordToken.Span.Start,
                    syntax.VariableDeclaration.IdentifierToken.Span.End));
            _diagnostics.ReportVariableAlreadyDeclared(location, name);
        }

        return new BoundVariableDeclarationStatement(variable, initializer);
    }

    TypeSymbol? BindTypeClause(TypeClauseSyntax? typeClause)
    {
        if (typeClause is null)
            return null;

        var type = LookupType(typeClause.Identifier.Text);
        if (type != null)
            return type;

        _diagnostics.ReportUndefinedType(typeClause.Identifier.Location, typeClause.Identifier.Text);
        return type;
    }

    BoundVariableDeclarationStatement BindVariableDeclarationStatement(
        VariableDeclarationStatementSyntax syntax)
    {
        return BindVariableDeclarationAssignmentSyntax(syntax.VariableDeclaration);
    }

    BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
    {
        var expression = BindExpression(syntax.Expression, true);
        return new BoundExpressionStatement(expression);
    }

    BoundBlockStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        _scope = new(_scope);
        foreach (var statementSyntax in syntax.Statements)
        {
            var statement = BindStatement(statementSyntax);
            statement.AddTo(statements);
        }

        _scope = _scope.Parent ?? throw new InvalidOperationException();

        return new(statements.ToImmutable());
    }

    BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol expectedType,
        bool allowExplicitConversion = false)
        => BindConversion(syntax, expectedType, allowExplicitConversion);

    BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
    {
        var result = BindExpressionInternal(syntax);
        if (!canBeVoid && result.Type == TypeSymbol.Void)
        {
            _diagnostics.ReportExpressionMustHaveValue(syntax.Location);
            return new BoundErrorExpression();
        }

        return result;
    }

    public BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.LiteralExpression:
                return BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.UnaryExpression:
                return BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.ParenthesizedExpression:
                return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return BindNameExpression((NameExpressionSyntax)syntax);
            case SyntaxKind.CallExpression:
                return BindCallExpression((CallExpressionSyntax)syntax);
            case SyntaxKind.AssignmentExpression:
                return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    BoundExpression BindCallExpression(CallExpressionSyntax syntax)
    {
        if (syntax.Arguments.Count == 1
            && LookupType(syntax.Identifier.Text) is { } type)
        {
            return BindConversion(syntax.Arguments[0], type, allowExplicitConversion: true);
        }

        if (!_scope.TryLookupFunction(syntax.Identifier.Text, out var function))
        {
            _diagnostics.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (function.Parameters.Length != syntax.Arguments.Count)
        {
            _diagnostics.ReportParameterCountMismatch(
                syntax.Identifier.Location,
                syntax.Identifier.Text,
                function.Parameters.Length,
                syntax.Arguments.Count);
            return new BoundErrorExpression();
        }

        var arguments = syntax.Arguments
            .Select((x, i) => BindExpression(x, function.Parameters[i].Type))
            .ToImmutableArray();
        return new BoundCallExpression(function, arguments);
    }

    BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicitConversion = false)
    {
        var expression = BindExpression(syntax);
        var diagnosticSpan = syntax.Location;
        return BindConversion(expression, type, diagnosticSpan, allowExplicitConversion);
    }

    BoundExpression BindConversion(BoundExpression expression, TypeSymbol type, TextLocation diagnosticLocation,
        bool allowExplicit = false)
    {
        var conversion = Conversion.Classify(expression.Type, type);

        if (conversion.IsIdentity)
            return expression;

        if ((!conversion.IsImplicit && !allowExplicit) || !conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
            {
                if (!allowExplicit && !conversion.IsImplicit && conversion.Exists)
                    _diagnostics.ReportNoImplicitConversion(diagnosticLocation, expression.Type, type);
                else
                    _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
            }

            return new BoundErrorExpression();
        }

        return new BoundConversionExpression(type, expression);
    }

    BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var boundExpression = BindExpression(syntax.Expression);
        var name = syntax.IdentifierToken.Text;

        if (!_scope.TryLookupVariable(name, out var variable))
        {
            _diagnostics.VariableDoesntExistsInCurrentScope(syntax.IdentifierToken.Location, name);
            return boundExpression;
        }

        if (variable.IsReadonly)
        {
            var span = TextSpan.FromBounds(syntax.IdentifierToken.Span.Start, syntax.EqualsToken.Span.End);
            var location = new TextLocation(syntax.SyntaxTree.SourceText, span);
            _diagnostics.ReportCannotAssignToReadonly(
                location,
                name);
        }

        boundExpression = BindConversion(boundExpression, variable.Type, syntax.Expression.Location);

        return new BoundAssignmentExpression(variable, boundExpression);
    }

    BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;

        if (name == string.Empty)
        {
            // this token was inserted by the parser to recover from an error
            // so error already reported and we can just return an error expression
            return new BoundErrorExpression();
        }

        if (!_scope.TryLookupVariable(name, out var variable))
        {
            _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Location, name);
            return new BoundErrorExpression();
        }

        return new BoundVariableExpression(variable);
    }

    BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }


    BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = BindExpression(syntax.Operand);
        var unaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);

        if (operand.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        if (unaryOperator is null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                operand.Type);
            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(unaryOperator, operand);
    }

    BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);

        if (left.Type == TypeSymbol.Error || right.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        var binaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
        if (binaryOperator is null)
        {
            _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                left.Type, right.Type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(left, binaryOperator, right);
    }

    BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value;
        return new BoundLiteralExpression(value, TypeSymbol.FromLiteral(syntax.LiteralToken));
    }

    TypeSymbol? LookupType(string name)
    {
        if (name == TypeSymbol.Bool.Name)
            return TypeSymbol.Bool;
        if (name == TypeSymbol.Int.Name)
            return TypeSymbol.Int;
        if (name == TypeSymbol.String.Name)
            return TypeSymbol.String;


        return null;
    }
}