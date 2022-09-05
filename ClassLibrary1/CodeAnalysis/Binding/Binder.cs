using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly DiagnosticBag diagnostics = new();
    private readonly BoundScope scope;
    public IEnumerable<Diagnostic> Diagnostics => this.diagnostics;
    
    public Binder(BoundScope parent)
    {
        this.scope = parent;
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous,CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(parentScope);
        var expression = binder.BindExpression(syntax.Expression);
        var variables = binder.scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();
        if (previous is not null) 
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
        
        return new BoundGlobalScope(previous, diagnostics, variables, expression);
    }
    
    private static BoundScope CreateParentScopes(BoundGlobalScope? previous)
    {
        if (previous is null)
        {
            return new BoundScope(null);
        }
        var stack = new Stack<BoundGlobalScope>();
        while (previous is not null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        BoundScope parent = null!;
        
        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach (var variable in previous.Variables) 
                scope.TryDeclareVariable(variable);
            
            parent = scope;
        }

        return parent;
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        switch (syntax.Kind)
        {
            case SyntaxKind.LiteralExpression:
                return this.BindLiteralExpression((LiteralExpressionSyntax)syntax);
            case SyntaxKind.UnaryExpression:
                return this.BindUnaryExpression((UnaryExpressionSyntax)syntax);
            case SyntaxKind.BinaryExpression:
                return this.BindBinaryExpression((BinaryExpressionSyntax)syntax);
            case SyntaxKind.ParenthesizedExpression:
                return this.BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
            case SyntaxKind.NameExpression:
                return this.BindNameExpression((NameExpressionSyntax)syntax);
            case SyntaxKind.AssignmentExpression:
                return this.BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var boundExpression = this.BindExpression(syntax.Expression);
        var name = syntax.IdentifierToken.Text;


        if (!this.scope.TryLookupVariable(name, out var variable))
        {
            variable = new VariableSymbol(name, boundExpression.Type);
            if (!this.scope.TryDeclareVariable(variable))
            {
                this.diagnostics.ReportVariableAlreadyDeclared(syntax.IdentifierToken.Span, name);
                return new BoundAssignmentExpression(variable, boundExpression);
            }
        }
        
        if (boundExpression.Type != variable.Type)
        {
            this.diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
            return boundExpression;
        }
        return new BoundAssignmentExpression(variable, boundExpression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        
        if (!this.scope.TryLookupVariable(name, out var variable))
        {
            this.diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return new BoundLiteralExpression(new object());
        }
        
        return new BoundVariableExpression(variable);
        
    }
    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return this.BindExpression(syntax.Expression);
    }


    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = this.BindExpression(syntax.Operand);
        var unaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, operand.Type);
        if (unaryOperator is null)
        {
            
            this.diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, operand.Type);
            return operand;
        }
        return new BoundUnaryExpression(unaryOperator, operand);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var left = this.BindExpression(syntax.Left);
        var right = this.BindExpression(syntax.Right);
        var binaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
        if (binaryOperator is null)
        {
            this.diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, left.Type, right.Type);
            return left;
        }
        return new BoundBinaryExpression(left, binaryOperator, right);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value;
        return new BoundLiteralExpression(value);
    }
}