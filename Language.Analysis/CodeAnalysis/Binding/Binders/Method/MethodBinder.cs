using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;
using OneOf;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Method;

sealed class MethodBinder
{
    BoundScope _scope;
    private readonly BoundScope _globalScope;
    readonly DiagnosticBag _diagnostics = new();
    readonly TypeSymbol _containingType;
    readonly MethodSymbol _currentMethod;

    readonly Stack<(LabelSymbol BreakLabel, LabelSymbol ContinueLabel)> _loopStack = new();

    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public MethodBinder(BoundScope scope, BoundScope globalScope, TypeSymbol containingType, MethodSymbol currentMethod)
    {
        _scope = scope;
        _globalScope = globalScope;
        _containingType = containingType;
        _currentMethod = currentMethod;
    }

    public ImmutableArray<Symbol> LookupSymbols(string name, BoundScope scope, TypeSymbol type)
    {
        var symbols = new List<Symbol>();
        var methods = type.LookupMethod(name);
        symbols.AddRange(methods);

        var field = type.LookupField(name);
        if (field.IsSome)
            symbols.Add(field.Unwrap());
        
        scope.TryLookupVariable(name, out var variable);
        if (variable != null) 
            symbols.Add(variable);

        var scopeType = scope.TryLookupType(name, _containingType.ContainingNamespace);
        if (scopeType.IsSome)
            symbols.Add(scopeType.Unwrap());

        var ns = _globalScope.TryLookupNamespace(name);
        if (ns.IsSome)
            symbols.Add(ns.Unwrap());
        
        return symbols.ToImmutableArray();
    }
    
    public BoundBlockStatement BindMethodBody(MethodSymbol methodSymbol)
    {
        _scope = _scope.CreateChild();
        BoundBlockStatement result;
        if (methodSymbol.Name is SyntaxFacts.MAIN_METHOD_NAME or SyntaxFacts.SCRIPT_MAIN_METHOD_NAME
            && _containingType.Name == SyntaxFacts.START_TYPE_NAME)
        {
            // method may be generated from global statements
            // so it needs special handling
            result = BindMainMethodBody(methodSymbol);
        }
        else
        {
            result = BindMethodBodyInternal(methodSymbol);
        }
        
        
        _scope = _scope.Parent.Unwrap();

        return result;
    }

    public BoundBlockStatement BindMethodBodyInternal(MethodSymbol methodSymbol)
    {
        methodSymbol.Parameters.ForEach(x => _scope.TryDeclareVariable(x));
        var result = BindBlockStatement(methodSymbol.DeclarationSyntax.Unwrap().As<MethodDeclarationSyntax>().Body);
        return result;
    }
    

    BoundBlockStatement BindMainMethodBody(MethodSymbol mainMethodSymbol)
    {
        // simple main method case
        return BindMethodBodyInternal(mainMethodSymbol);
    }
    
    BoundStatement BindStatement(StatementSyntax syntax, bool isGlobal = false)
    {
        var result = BindStatementInternal(syntax);
  
        if (result is not BoundExpressionStatement es)
            return result;

        var isAllowedExpression = es.Expression.Kind
            is BoundNodeKind.AssignmentExpression
            or BoundNodeKind.MethodCallExpression
            or BoundNodeKind.MemberAssignmentExpression
            or BoundNodeKind.ErrorExpression;
        if (!isAllowedExpression && es.Expression.Kind is BoundNodeKind.MemberAccessExpression)
        {
            var memberAccess = (BoundMemberAccessExpression)es.Expression;
            if (memberAccess.Member.Kind is BoundNodeKind.FieldExpression) 
                isAllowedExpression = false;
            else
                isAllowedExpression = true;
        }
        if (!isAllowedExpression)
        {
            _diagnostics.ReportInvalidExpressionStatement(syntax.Location);
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
            case SyntaxKind.VariableDeclarationAssignmentStatement:
                return BindVariableDeclarationAssignmentStatement((VariableDeclarationAssignmentStatementSyntax)syntax);
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


        if (Equals(_currentMethod.ReturnType, TypeSymbol.BuiltIn.Void()))
        {
            if (expression is not null)
                _diagnostics.ReportReturnStatementIsInvalidForVoidMethod(syntax.Location);
        }
        else
        {
            if (expression is null)
            {
                _diagnostics.ReportReturnStatementIsInvalidForNonVoidMethod(syntax.Location,
                    _currentMethod.ReturnType);
            }
            else
            {
                Debug.Assert(syntax.Expression != null, "syntax.Expression != null");
                expression = BindConversion(expression, _currentMethod.ReturnType, syntax.Expression.Location);
            }
        }


        return new BoundReturnStatement(expression.NullGuard().Syntax, expression);
    }

    BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreakOrContinue(syntax.BreakKeyword);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(syntax, _loopStack.Peek().BreakLabel);
    }

    BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
    {
        if (_loopStack.Count == 0)
        {
            _diagnostics.ReportInvalidBreakOrContinue(syntax.ContinueKeyword);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(syntax, _loopStack.Peek().ContinueLabel);
    }

    BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        BoundVariableDeclarationAssignmentStatement? variableDeclaration = null;
        BoundExpression? expression = null;
        if (syntax.VariableDeclaration is not null)
            variableDeclaration = BindVariableDeclarationAssignmentSyntax(syntax.VariableDeclaration);
        else
            expression = BindExpression(syntax.Expression.NullGuard());

        var condition = BindExpression(syntax.Condition, TypeSymbol.BuiltIn.Bool());
        var mutation = BindExpression(syntax.Mutation);

        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

        return new BoundForStatement(syntax, variableDeclaration, expression, condition, mutation, body, breakLabel, continueLabel);
    }

    BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.BuiltIn.Bool());
        var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
        return new BoundWhileStatement(syntax, condition, body, breakLabel, continueLabel);
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
        var condition = BindExpression(syntax.Condition, TypeSymbol.BuiltIn.Bool());
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause is null
            ? null
            : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(syntax, condition, thenStatement, elseStatement);
    }

    TypeSymbol? BindTypeClause(TypeClauseSyntax? typeClause)
    {
        if (typeClause is null)
            return null;

        var type = TypeSymbol.FromNamedTypeExpression(typeClause.NamedTypeExpression, _scope, _diagnostics, _containingType.ContainingNamespace);
        
        if (Equals(type, TypeSymbol.BuiltIn.Error()))
            return null;
        
        return type;
    }

    BoundStatement BindVariableDeclarationStatement(VariableDeclarationStatementSyntax syntax) 
        => BindVariableDeclarationSyntax(syntax.VariableDeclaration, variableType: null);

    BoundVariableDeclarationStatement BindVariableDeclarationSyntax(VariableDeclarationSyntax currentVariableDeclarationSyntax, TypeSymbol? variableType)
    {
        if (currentVariableDeclarationSyntax.TypeClause is null && variableType is null)
            _diagnostics.ReportTypeClauseExpected(currentVariableDeclarationSyntax.Identifier.Location);
        
        var isReadonly = currentVariableDeclarationSyntax.KeywordToken.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(currentVariableDeclarationSyntax.TypeClause);
        
        var name = currentVariableDeclarationSyntax.Identifier.Text;
        var variable = new VariableSymbol(
            currentVariableDeclarationSyntax,
            name,
            (type ?? variableType) ?? TypeSymbol.BuiltIn.Error(),
            isReadonly);

        if (!_scope.TryDeclareVariable(variable))
        {

            _scope.TryLookupVariable(variable.Name, out var existingSymbol)
                .EnsureTrue();
            var existingVariables = _currentMethod.Parameters.Append(existingSymbol);

            foreach (var existingVariable in existingVariables)
            {
                SyntaxToken? existingVariableIdentifier;
                switch (existingVariable.NullGuard().Kind)
                {
                    case SymbolKind.Parameter:
                    {
                        existingVariableIdentifier =
                            existingVariable.DeclarationSyntax.Unwrap().As<ParameterSyntax>().Identifier;
                        _diagnostics.ReportVariableAlreadyDeclared(existingVariableIdentifier);
                        _diagnostics.ReportParameterAlreadyDeclared(currentVariableDeclarationSyntax.Identifier);
                        break;
                    }
                    case SymbolKind.Variable:
                    {
                        existingVariableIdentifier = existingVariable.DeclarationSyntax
                            .UnwrapAs<VariableDeclarationSyntax>().Identifier;
                        _diagnostics.ReportVariableAlreadyDeclared(existingVariableIdentifier);
                        _diagnostics.ReportVariableAlreadyDeclared(currentVariableDeclarationSyntax.Identifier);
                        break;
                    }
                    default:
                        throw new Exception($"Unexpected symbol {existingVariable.NullGuard().Kind}");
                }
            }
            
        }

        return new BoundVariableDeclarationStatement(currentVariableDeclarationSyntax, variable);
    }
    
    BoundVariableDeclarationAssignmentStatement BindVariableDeclarationAssignmentStatement(
        VariableDeclarationAssignmentStatementSyntax syntax) 
        => BindVariableDeclarationAssignmentSyntax(syntax.VariableDeclaration);
    BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
    {
        var expression = BindExpression(syntax.Expression, true);
        return new BoundExpressionStatement(syntax, expression);
    }
    
    BoundVariableDeclarationAssignmentStatement BindVariableDeclarationAssignmentSyntax(
        VariableDeclarationAssignmentSyntax syntax)
    {
        var initializer = BindExpression(syntax.Initializer);
        var variable = BindVariableDeclarationSyntax(syntax.VariableDeclaration, initializer.Type);
        
        initializer = BindConversion(initializer, variable.Variable.Type, syntax.Initializer.Location);

        return new BoundVariableDeclarationAssignmentStatement(syntax, variable.Variable, initializer);
    }

    BoundBlockStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var statementSyntax in syntax.Statements)
        {
            var statement = BindStatement(statementSyntax);
            statement.AddTo(statements);
        }

        return new(syntax, statements.ToImmutable());
    }

    BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol expectedType,
        bool allowExplicitConversion = false)
        => BindConversion(syntax, expectedType, allowExplicitConversion);

    BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
    {
        var result = BindExpressionInternal(syntax);
        if (!canBeVoid && Equals(result.Type, TypeSymbol.BuiltIn.Void()))
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
                return BindParenthesizedExpression(syntax.As<ParenthesizedExpressionSyntax>());
            case SyntaxKind.CastExpression:
                return BindCastExpression(syntax.As<CastExpressionSyntax>());
            case SyntaxKind.NameExpression:
                return BindNameExpression((NameExpressionSyntax)syntax,
                                          type: _containingType,
                                          // if method is static, it can be only static member access
                                          _currentMethod.IsStatic);
            case SyntaxKind.ThisExpression:
                return BindThisExpression((ThisExpressionSyntax)syntax);
            case SyntaxKind.ObjectCreationExpression:
                return BindNewExpression((NewExpressionSyntax)syntax);
            case SyntaxKind.MemberAccessExpression:
                return BindMemberAccessExpression((MemberAccessExpressionSyntax)syntax);
            case SyntaxKind.MemberAssignmentExpression:
                return BindMemberAssignmentExpression((MemberAssignmentExpressionSyntax)syntax);
            case SyntaxKind.MethodCallExpression:
                return BindMethodCallExpression((MethodCallExpressionSyntax)syntax, 
                                                _containingType,
                                                // if method is static, it can be only static member access
                                                isCalledOnStatic: _currentMethod.IsStatic);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    BoundExpression BindCastExpression(CastExpressionSyntax castExpressionSyntax)
    {
        var castType = _scope.TryLookupType(castExpressionSyntax.NameExpression.Identifier.Text, _containingType.ContainingNamespace); 
        if (castType.IsNone)
        {
            _diagnostics.ReportUndefinedType(castExpressionSyntax.NameExpression.Identifier.Location, castExpressionSyntax.NameExpression.Identifier.Text);
            return new BoundErrorExpression();
        }
        var expression = BindExpression(castExpressionSyntax.CastedExpression, castType.Unwrap(), true);
        
        return expression;
    }

    BoundExpression BindThisExpression(ThisExpressionSyntax syntax)
    {
        if (_currentMethod.IsStatic)
        {
            _diagnostics.ReportThisExpressionNotAllowedInStaticContext(syntax.ThisKeyword);
        }
        return new BoundThisExpression(syntax, _containingType);
    }

    BoundExpression BindMemberAssignmentExpression(MemberAssignmentExpressionSyntax syntax)
    {
        
        BoundExpression member;
        if (syntax.MemberAccess.Kind is SyntaxKind.NameExpression)
        {
            var nameExpression = (NameExpressionSyntax)syntax.MemberAccess;
            member = BindNameExpression(nameExpression,
                                        _containingType,
                                        isCalledOnStatic: _currentMethod.IsStatic);
            if (member.Kind is BoundNodeKind.VariableExpression)
            {
                var variableExpression = (BoundVariableExpression)member;
                if (variableExpression.Variable.IsReadonly)
                {
                    _diagnostics.ReportCannotAssignToReadonly(nameExpression.Identifier);
                }
            }
        }
        else
        {
            member = BindMemberAccessExpression(syntax.MemberAccess);   
        }
        
        if (member is BoundErrorExpression)
            return new BoundErrorExpression();

        var rightValue = BindExpression(syntax.Initializer, member.Type);
        return new BoundMemberAssignmentExpression(syntax, member, rightValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="methodCallExpressionSyntax"></param>
    /// <param name="type"></param>
    /// <param name="isCalledOnStatic"></param>
    /// <returns></returns>
    BoundExpression BindMethodCallExpression(MethodCallExpressionSyntax methodCallExpressionSyntax, TypeSymbol type, bool isCalledOnStatic)
    {
        var methods = type.LookupMethod(methodCallExpressionSyntax.Identifier.Text);
        var methodSymbol = methods.FirstOrDefault();
        if (methodSymbol is null)
        {
            _diagnostics.ReportUndefinedMethod(methodCallExpressionSyntax.Identifier, type);
            return new BoundErrorExpression();
        }
        var arguments = methodCallExpressionSyntax.Arguments.Select(x => BindExpression(x)).ToImmutableArray();
        var boundMethodCall = new BoundMethodCallExpression(methodCallExpressionSyntax, methodSymbol, arguments);
        
        
        if (isCalledOnStatic is false && methodSymbol.IsStatic)
        {
            _diagnostics.ReportCannotAccessStaticFieldOnNonStaticMember(methodCallExpressionSyntax.Identifier);
        }

        if (methodSymbol.IsGeneric)
        {
            if (methodCallExpressionSyntax.GenericClause.IsNone)
            {
                _diagnostics.ReportGenericMethodGenericArgumentsNotSpecified(methodCallExpressionSyntax.Identifier);
            }
            else
            {
                var genericParameters = methodSymbol.GenericParameters.Unwrap().ToList();
                if (genericParameters.Count != methodCallExpressionSyntax.GenericClause.Unwrap().Arguments.Count)
                {
                    _diagnostics.ReportGenericMethodCallWithWrongTypeArgumentsCount(methodCallExpressionSyntax.Identifier, 
                                                                                    methodCallExpressionSyntax.GenericClause.Unwrap(),
                                                                                    genericParameters);
                }

                var genericParametersTypes = genericParameters.ToList();
                CheckGenericTypeConstraintsForGenericClassMemberInvocation(genericParametersTypes, methodCallExpressionSyntax);
            }
        }



        return boundMethodCall;
    }

    /// <summary>
    /// Count of generic arguments expected to match count of generic parameters.
    /// </summary>
    /// <param name="genericParametersOfClass"></param>
    /// <param name="invocationSyntax"></param>
    /// <exception cref="Exception">Will be thrown when count of generic parameters doesn't match count of generic arguments.</exception>
    void CheckGenericTypeConstraintsForGenericClassMemberInvocation(List<TypeSymbol> genericParametersOfClass, OneOf<MethodCallExpressionSyntax, NewExpressionSyntax> invocationSyntax)
    {
        var optionalGenericArguments = invocationSyntax.Match<Option<SeparatedSyntaxList<NamedTypeExpressionSyntax>>>(
            methodCallES =>
            {
                var genericArguments = methodCallES.GenericClause.Unwrap().Arguments;
                if (genericArguments.Count != genericParametersOfClass.Count)
                {
                    _diagnostics.ReportGenericMethodCallWithWrongTypeArgumentsCount(methodCallES.Identifier, methodCallES.GenericClause.Unwrap(), genericParametersOfClass);
                    return Option.None;
                }
                return genericArguments;
            }, 
            newExpressionSyntax => 
            {
                var genericArguments = newExpressionSyntax.NamedTypeExpression.GenericClause.Unwrap().Arguments;
                if (genericArguments.Count != genericParametersOfClass.Count)
                {
                    _diagnostics.NewExpressionGenericArgumentsWrongCount(newExpressionSyntax, genericParametersOfClass);
                    return Option.None;
                }
                return genericArguments;
            });

        if (optionalGenericArguments.IsNone)
            return;
        
        SeparatedSyntaxList<NamedTypeExpressionSyntax> genericArguments = optionalGenericArguments.Unwrap();
        TypeSymbol.CheckGenericConstraints(genericParametersOfClass, genericArguments.ToList(), _scope, _diagnostics, _containingType.ContainingNamespace);
    }

    string GetFullNamespaceNameFromMemberAccess(MemberAccessExpressionSyntax memberAccess)
    {
        
        var right = (NameExpressionSyntax)memberAccess.Right;
        var name = right.Identifier.Text;
        ExpressionSyntax cur = memberAccess;
        
        while (cur is MemberAccessExpressionSyntax { Left.Kind: not SyntaxKind.NameExpression } curMa)
        {
            cur = curMa.Left;
            name = cur.As<MemberAccessExpressionSyntax>().Right.As<NameExpressionSyntax>().Identifier.Text + "." + name;
        }

        return cur.As<MemberAccessExpressionSyntax>().Left.As<NameExpressionSyntax>().Identifier.Text + "." + name;
    }
    BoundExpression BindMemberAccessExpression(ExpressionSyntax syntax)
    {
        if (syntax.Kind is SyntaxKind.NameExpression)
            return BindNameExpression((NameExpressionSyntax)syntax, 
                                      _containingType,
                                      isCalledOnStatic: _currentMethod.IsStatic);

        if (syntax.Kind is SyntaxKind.MethodCallExpression)
            return BindMethodCallExpression((MethodCallExpressionSyntax)syntax,
                                            _containingType,
                                            isCalledOnStatic: _currentMethod.IsStatic);

        Debug.Assert(
            syntax.Kind is SyntaxKind.MemberAccessExpression,
            $"(syntax.Kind is SyntaxKind.MemberAccessExpression) in {SourceTextMeta.GetCurrentInvokeLocation()}");

        
        BoundExpression left;
        bool isStatic = false;
        var memberAccess = (MemberAccessExpressionSyntax)syntax;
        if (memberAccess.Left.Kind is SyntaxKind.NameExpression)
        {
            var nameExpression = (NameExpressionSyntax)memberAccess.Left;
            var symbols = LookupSymbols(nameExpression.Identifier.Text, _scope, _containingType);
            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndefinedName(nameExpression.Identifier.Location, nameExpression.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (symbols.Length is 1)
            {
                var symbol = symbols[0];
                isStatic = symbol.Kind is SymbolKind.Type;
                left = symbol.Kind switch
                {
                    SymbolKind.Field
                        => new BoundFieldExpression(nameExpression, (FieldSymbol)symbol),
                    SymbolKind.Variable or SymbolKind.Parameter 
                        => new BoundVariableExpression(nameExpression, (VariableSymbol)symbol),
                    SymbolKind.Type 
                        => new BoundNamedTypeExpression(nameExpression, (TypeSymbol)symbol),
                    SymbolKind.Namespace => new BoundNamespaceExpression(nameExpression, (NamespaceSymbol)symbol),
                    _ => throw new Exception($"Unexpected symbol {symbol.Kind}")
                };
            }
            else
            {
                var fieldSymbol = symbols.Where(x => x.Kind is SymbolKind.Field).ToList();
                var typeSymbol = symbols.Where(x => x.Kind is SymbolKind.Type).ToList();
                var variableSymbol = symbols.Where(x=> x.Kind is SymbolKind.Parameter or SymbolKind.Variable).ToList();
                var inferResult = TryInferLeftExpressionSymbolFromRightSide(variableSymbol.Concat(fieldSymbol).Concat(typeSymbol),
                                                                            memberAccess.Right, out var bestMatchSymbol);
                if (inferResult is InferResult.Success)
                {
                    bestMatchSymbol.NullGuard();
                    left = BindNameExpressionFromSymbol(nameExpression, bestMatchSymbol);
                    isStatic = bestMatchSymbol.Kind is SymbolKind.Type;
                }
                else if (inferResult is InferResult.Ambiguous)
                {
                    var identifier = memberAccess.Right.Kind switch
                    {
                        SyntaxKind.NameExpression => ((NameExpressionSyntax)memberAccess.Right).Identifier,
                        SyntaxKind.MethodCallExpression => ((MethodCallExpressionSyntax)memberAccess.Right).Identifier,
                        _ => throw new Exception($"Unexpected syntax {memberAccess.Right.Kind}")
                    };
                    
                    _diagnostics.ReportAmbiguousMemberMemberAccess(identifier, symbols);
                    return new BoundErrorExpression();
                }
                else
                {
                    // do not report diagnostics, because we will report undefined name error later
                    // when we try to bind the right side
                    bestMatchSymbol.NullGuard();
                    left = BindNameExpressionFromSymbol(nameExpression, bestMatchSymbol);
                }
            }
        }
        else
        {
            left = BindExpression(memberAccess.Left);
        }

        if (left.Kind is BoundNodeKind.NamespaceExpression)
        {
            if (memberAccess.Right.Kind is not SyntaxKind.NameExpression)
            {
                // TODO
                _diagnostics.Report(memberAccess.Location, "<TODO> Unknown access, expected Type got no", "TODO CODE");
                return new BoundErrorExpression();
            }

            var ns = LookupSymbols(GetFullNamespaceNameFromMemberAccess(memberAccess), _scope, _containingType);
            var right = (NameExpressionSyntax)memberAccess.Right;
            var type = ((BoundNamespaceExpression)left).NamespaceSymbol.LookupType(right.Identifier.Text);
            if (type.IsNone)
            {
                _diagnostics.ReportUndefinedType(memberAccess.Location, right.Identifier.Text); // TODO
                return new BoundErrorExpression();
            }
            return new BoundNamedTypeExpression(memberAccess, type.Unwrap());

        }
        else
        {
            if (memberAccess.Right.Kind is SyntaxKind.MethodCallExpression)
            {
                if (left.Kind is BoundNodeKind.NamedTypeExpression)
                    isStatic = true;
                
                var methodCall = BindMethodCallExpression((MethodCallExpressionSyntax)memberAccess.Right, left.Type, isStatic);
            
                return new BoundMemberAccessExpression(syntax, left, methodCall);
            }

            if (memberAccess.Right.Kind is SyntaxKind.NameExpression)
            {
            
                var nameExpression = (NameExpressionSyntax)memberAccess.Right;
                var fieldAccess = BindNameExpression(nameExpression, left.Type, isStatic);
            
                return new BoundMemberAccessExpression(memberAccess, left, fieldAccess);
            }
        }

        throw new Exception($"Unexpected syntax {memberAccess.Right.Kind}");
    }


    internal enum InferResult
    {
        /// <summary>
        /// No match found
        /// </summary>
        None,
        /// <summary>
        /// Found multiple matches
        /// </summary>
        Ambiguous,
        /// <summary>
        /// Found a single match
        /// </summary>
        Success
    }
    
    /// <summary>
    ///     Takes a list of symbols and tries to infer the best match based on the right side of the member access expression.<br/>
    ///     Does not report any diagnostics. <br/>
    ///     Does not check if the right side is valid for the inferred symbol. <br/>
    ///     If no match is found, <paramref name="matchingSymbol"/> is first symbol according to <see cref="SymbolSorter.GetSorted"/>. <br/>
    /// </summary>
    /// <param name="symbols">
    ///     Allowed symbols: <br/>
    ///     <see cref="FieldSymbol"/>, <br/>
    ///     <see cref="ParameterSymbol"/>, <br/>
    ///     <see cref="VariableSymbol"/>, <br/>
    ///     <see cref="TypeSymbol"/>.
    /// </param>
    /// <param name="right">
    ///     Right side of member access expression. <br/>
    ///     Allowed syntax: <br/>
    ///     <see cref="SyntaxKind.NameExpression"/>, <br/>
    ///     <see cref="SyntaxKind.MethodCallExpression"/>.
    /// </param>
    /// <param name="matchingSymbol">symbol best matching for right side.</param>
    /// <returns>true if symbol can be inferred, false otherwise.</returns>
    internal InferResult TryInferLeftExpressionSymbolFromRightSide(IEnumerable<Symbol> symbols,
                                                                   ExpressionSyntax right, 
                                                                   out Symbol? matchingSymbol)
    {
        var symbolList = symbols.ToList();
        symbolList.Any().EnsureTrue("No symbols to infer from.");
        symbolList.All(
            x => x.Kind 
                is SymbolKind.Field 
                or SymbolKind.Parameter 
                or SymbolKind.Variable 
                or SymbolKind.Type
                && x is ITypedSymbol)
            .EnsureTrue("Unexpected symbol kind.");
        
        (right.Kind is SyntaxKind.NameExpression or SyntaxKind.MethodCallExpression)
            .EnsureTrue($"Unexpected right side expression {right.Kind}");
        
        if (symbolList.Count is 1)
        {
            matchingSymbol = symbolList.Single();
            // we don't have to check if right side is valid for this symbol
            // because it should be be checked later in right side binding.
            return InferResult.Success;
        }

        if (right.Kind is SyntaxKind.NameExpression)
        {
            var nameExpression = (NameExpressionSyntax)right;
            
            // types is checked differently, because if accessing member on typeSymbol,
            // then the right side should be a static.
            var matchingTypes = symbolList.OfType<TypeSymbol>()
                .Where(x => x.FieldTable.Any(field => field.IsStatic && field.Name == nameExpression.Identifier.Text))
                .ToList();
            
            
            var matchingOther = symbolList
                .OfType<ITypedSymbol>()
                .Where(x => x.Type.FieldTable.Any(field => !field.IsStatic && field.Name == nameExpression.Identifier.Text))
                .ToList();
            
            var matchingSymbols = matchingTypes.Concat(matchingOther).Cast<Symbol>().ToList();
            if (matchingSymbols.Count is 1)
            {
                matchingSymbol = matchingSymbols.Single();
                return InferResult.Success;
            }

            if (matchingSymbols.Count > 1)
            {
                matchingSymbol = null;
                return InferResult.Ambiguous;
            }

            matchingSymbol = null;
            return InferResult.None;
        }
        
        if (right.Kind is SyntaxKind.MethodCallExpression)
        {
            var methodCall = (MethodCallExpressionSyntax)right;
            
            // types is checked differently, because if accessing member on typeSymbol,
            // then the right side should be a static.
            var matchingTypes = symbolList.OfType<TypeSymbol>()
                .Where(x => x.MethodTable
                           .Any(method => method.MethodSymbol.IsStatic && method.MethodSymbol.Name == methodCall.Identifier.Text));
            
            var matchingOther = symbolList
                .OfType<ITypedSymbol>()
                .Where(x => x.Type.MethodTable
                           .Any(method => !method.MethodSymbol.IsStatic && method.MethodSymbol.Name == methodCall.Identifier.Text));
            
            var matchingSymbols = matchingTypes.Concat(matchingOther).Cast<Symbol>().ToList();
            if (matchingSymbols.Count is 1)
            {
                matchingSymbol = matchingSymbols.Single();
                return InferResult.Success;
            }
            if (matchingSymbols.Count > 1)
            {
                matchingSymbol = null;
                return InferResult.Ambiguous;
            }

            var sortedSymbols = SymbolSorter.GetSorted(symbolList);
            matchingSymbol = sortedSymbols.First();
            return InferResult.None;
        }
        
        throw new Exception($"Should never throw. Unexpected right side expression {right.Kind}");
    }


    BoundExpression BindNewExpression(NewExpressionSyntax syntax)
    {
        var type = TypeSymbol.FromNamedTypeExpression(syntax.NamedTypeExpression, 
                                                          _scope,
                                                          _diagnostics,
                                                          _containingType.ContainingNamespace);
        

        return new BoundObjectCreationExpression(syntax, type);
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
            if (expression.Type != TypeSymbol.BuiltIn.Error() && type != TypeSymbol.BuiltIn.Error())
            {
                if (!allowExplicit && !conversion.IsImplicit && conversion.Exists)
                    _diagnostics.ReportNoImplicitConversion(diagnosticLocation, expression.Type, type);
                else
                    _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
            }

            return new BoundErrorExpression();
        }

        return new BoundConversionExpression(null, type, expression);
    }

    BoundExpression BindNameExpressionFromSymbol(NameExpressionSyntax nameExpression, Symbol symbol)
        => symbol.Kind switch
        {
            SymbolKind.Field
                => new BoundFieldExpression(nameExpression, (FieldSymbol)symbol),
            SymbolKind.Variable or SymbolKind.Parameter
                => new BoundVariableExpression(nameExpression, (VariableSymbol)symbol),
            SymbolKind.Type
                => new BoundNamedTypeExpression(nameExpression, (TypeSymbol)symbol),

            _ => throw new Exception($"Unexpected symbol {symbol.Kind}")
        };

    BoundExpression BindNameExpression(NameExpressionSyntax syntax, TypeSymbol type, bool isCalledOnStatic)
    {
        var name = syntax.Identifier.Text;

        if (name == string.Empty)
        {
            // this token was inserted by the parser to recover from an error
            // so error already reported and we can just return an error expression
            return new BoundErrorExpression();
        }
        
        
        var field = type.LookupField(name);

        if (_scope.TryLookupVariable(name, out var variable))
        {
            return new BoundVariableExpression(syntax, variable);
        }

        if (field.IsNone && !type.Equals(_containingType))
        {
            _diagnostics.ReportUndefinedFieldAccess(syntax.Identifier, type);
            return new BoundErrorExpression();
        }
        if (field.IsNone)
        {
            _diagnostics.ReportUndefinedName(syntax.Identifier.Location, name);
            return new BoundErrorExpression();
        }

        if (isCalledOnStatic is false && field.Unwrap().IsStatic)
        {
            _diagnostics.ReportCannotAccessStaticFieldOnNonStaticMember(syntax.Identifier);
        }

        return new BoundFieldExpression(syntax, field.Unwrap());
    }

    BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }


    BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = BindExpression(syntax.Operand);
        var unaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);

        if (Equals(operand.Type, TypeSymbol.BuiltIn.Error()))
            return new BoundErrorExpression();

        if (unaryOperator is null)
        {
            _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                operand.Type);
            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(syntax, unaryOperator, operand);
    }

    BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var left = BindExpression(syntax.Left);
        var right = BindExpression(syntax.Right);

        if (Equals(left.Type, TypeSymbol.BuiltIn.Error()) || Equals(right.Type, TypeSymbol.BuiltIn.Error()))
            return new BoundErrorExpression();

        var binaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
        if (binaryOperator is null)
        {
            _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text,
                left.Type, right.Type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(syntax, left, binaryOperator, right);
    }

    BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value;
        return new BoundLiteralExpression(syntax, value, TypeSymbol.FromLiteral(syntax.LiteralToken));
    }

    BoundStatement BindErrorStatement()
        => new BoundExpressionStatement(null, new BoundErrorExpression());
}