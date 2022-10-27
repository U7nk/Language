using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class FunctionBinder
{
    BoundScope _scope;
    readonly DiagnosticBag _diagnostics = new();
    readonly bool _isScript;
    readonly FunctionBinderLookup _lookup;
    
    readonly Stack<(LabelSymbol BreakLabel, LabelSymbol ContinueLabel)> _loopStack = new();

    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public FunctionBinder(BoundScope scope, bool isScript, FunctionBinderLookup lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public BoundBlockStatement BindFunctionBody(FunctionSymbol functionSymbol)
    {
        _scope = new(_scope);
        functionSymbol.Parameters.ForEach(x => _scope.TryDeclareVariable(x));
        
        var result = BindBlockStatement(functionSymbol.Declaration.Unwrap().Body);
        
        _scope = _scope.Parent ?? throw new InvalidOperationException();
        
        return result;
    }

    public BoundStatement BindGlobalStatement(StatementSyntax syntax) => BindStatement(syntax, isGlobal: true);

    BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
    {
        var result = BindStatementInternal(syntax);
        if (!_isScript || !isGlobal)
        {
            if (result is BoundExpressionStatement es)
            {
                var isAllowedExpression = es.Expression.Kind
                    is BoundNodeKind.AssignmentExpression
                    or BoundNodeKind.MethodCallExpression
                    or BoundNodeKind.MemberAccessExpression
                    or BoundNodeKind.MemberAssignmentExpression
                    or BoundNodeKind.ErrorExpression;
                if (!isAllowedExpression)
                {
                    _diagnostics.ReportInvalidExpressionStatement(syntax.Location);
                }
            }
        }

        return result;
    }
    BoundStatement BindStatementInternal(StatementSyntax syntax)
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


        if (Equals(_lookup.CurrentFunction.ReturnType, TypeSymbol.Void))
        {
            if (expression is not null)
                _diagnostics.ReportReturnStatementIsInvalidForVoidFunction(syntax.Location);
        }
        else
        {
            if (expression is null)
            {
                if (_isScript)
                    expression = new BoundLiteralExpression("null", TypeSymbol.String);
                else
                {
                    _diagnostics.ReportReturnStatementIsInvalidForNonVoidFunction(syntax.Location,
                        _lookup.CurrentFunction.ReturnType);
                }
            }
            else
            {
                Debug.Assert(syntax.Expression != null, "syntax.Expression != null");
                expression = BindConversion(expression, _lookup.CurrentFunction.ReturnType, syntax.Expression.Location);
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
            expression = BindExpression(syntax.Expression.Unwrap());

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
            var location = syntax.VariableDeclaration.IdentifierToken.Location;
            _diagnostics.ReportVariableAlreadyDeclared(location, name);
        }

        return new BoundVariableDeclarationStatement(variable, initializer);
    }

    
    TypeSymbol? BindTypeClause(TypeClauseSyntax? typeClause)
    {
        if (typeClause is null)
            return null;

        _lookup.Unwrap();
        var type = _lookup.AvailableTypes.SingleOrDefault(x=> x.Name == typeClause.Identifier.Text);
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
        
        foreach (var statementSyntax in syntax.Statements)
        {
            var statement = BindStatement(statementSyntax);
            statement.AddTo(statements);
        }

        return new(statements.ToImmutable());
    }

    BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol expectedType,
        bool allowExplicitConversion = false)
        => BindConversion(syntax, expectedType, allowExplicitConversion);

    BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
    {
        var result = BindExpressionInternal(syntax);
        if (!canBeVoid && Equals(result.Type, TypeSymbol.Void))
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
            case SyntaxKind.BinaryOperatorExpression:
                return BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.ParenthesizedExpression:
                return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return BindNameExpression((NameExpressionSyntax)syntax);
            case SyntaxKind.ThisExpression:
                return BindThisExpression((ThisExpressionSyntax)syntax);
            case SyntaxKind.MethodCallExpression:
                return BindCallExpression((MethodCallExpressionSyntax)syntax);
            case SyntaxKind.AssignmentExpression:
                return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
            case SyntaxKind.ObjectCreationExpression:
                return BindObjectCreationExpression((ObjectCreationExpressionSyntax)syntax);
            case SyntaxKind.MemberAccessExpression:
                return BindMemberAccessExpression((MemberAccessExpressionSyntax)syntax);
            case SyntaxKind.MemberAssignmentExpression:
                return BindMemberAssignmentExpression((MemberAssignmentExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    BoundExpression BindThisExpression(ThisExpressionSyntax syntax) 
        => new BoundThisExpression(_lookup.CurrentType);

    BoundExpression BindMemberAssignmentExpression(MemberAssignmentExpressionSyntax syntax)
    {
        var member = BindMemberAccessExpression(syntax.MemberAccess);
        if (member is BoundErrorExpression)
            return new BoundErrorExpression();
        
        var rightValue = BindExpression(syntax.Initializer, member.Type);
        return new BoundMemberAssignmentExpression((BoundMemberAccessExpression)member, rightValue);
    }

    BoundExpression BindMemberAccessExpression(MemberAccessExpressionSyntax syntax)
    {
        var left = BindExpression(syntax.Left);
        if (syntax.Right.Kind is SyntaxKind.MethodCallExpression)
        {
            var methodCall = (MethodCallExpressionSyntax)syntax.Right;
            var method = left.Type.MethodTable.Keys.SingleOrDefault(x=> x.Name == methodCall.Identifier.Text);
            if (method is null)
            {
                _diagnostics.ReportUndefinedMethod(methodCall.Identifier, left.Type);
                return new BoundErrorExpression();
            }
            var arguments = methodCall.Arguments.Select(x=> BindExpression(x))
                .ToImmutableArray();
            
            var boundMethodCall = new BoundMethodCallExpression(method, arguments);
            return new BoundMemberAccessExpression(left, boundMethodCall);
        }
        if (syntax.Right.Kind is SyntaxKind.NameExpression)
        {
            var nameExpression = (NameExpressionSyntax)syntax.Right;
            var field = left.Type.FieldTable.SingleOrDefault(x => x.Name == nameExpression.IdentifierToken.Text);
            if (field is null)
            {
                _diagnostics.ReportUndefinedFieldAccess(nameExpression.IdentifierToken, left.Type);
                return new BoundErrorExpression();
            }
            var fieldAccess = new BoundFieldAccessExpression(field);
            return new BoundMemberAccessExpression(left, fieldAccess);
        }
        
        throw new Exception($"Unexpected syntax {syntax.Right.Kind}");
    }


    BoundExpression BindObjectCreationExpression(ObjectCreationExpressionSyntax syntax)
    {
        _lookup.Unwrap();

        var typeName = syntax.TypeIdentifier.Text;
        var matchingTypes = _lookup.AvailableTypes
            .Where(t => t.Name == typeName)
            .ToList();
        if (matchingTypes.Count > 1)
        {
            _diagnostics.ReportAmbiguousType(syntax.TypeIdentifier.Location, typeName, matchingTypes);
            return new BoundErrorExpression();
        }

        if (matchingTypes.Count < 1)
        {
            _diagnostics.ReportUndefinedType(syntax.TypeIdentifier.Location, typeName);
            return new BoundErrorExpression();
        }

        return new BoundObjectCreationExpression(matchingTypes.Single());
    }

    BoundExpression BindCallExpression(MethodCallExpressionSyntax syntax)
    {
        _lookup.Unwrap();
        if (syntax.Arguments.Count == 1
            && _lookup.AvailableTypes.SingleOrDefault(x=> x.Name == syntax.Identifier.Text) is { } type)
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
            .ToList();
        
        arguments.Insert(0, new BoundThisExpression(_lookup.Unwrap().CurrentType));
        return new BoundMethodCallExpression(function, arguments.ToImmutableArray());
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

        if (_scope.TryLookupVariable(name, out var variable))
        {
            return new BoundVariableExpression(variable);
        }

        _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Location, name);
        return new BoundErrorExpression();
    }

    BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }


    BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = BindExpression(syntax.Operand);
        var unaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);

        if (Equals(operand.Type, TypeSymbol.Error))
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

        if (Equals(left.Type, TypeSymbol.Error) || Equals(right.Type, TypeSymbol.Error))
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

    BoundStatement BindErrorStatement()
        => new BoundExpressionStatement(new BoundErrorExpression());
}