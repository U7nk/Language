using System;
using System.Collections.Generic;
using System.Linq;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly DiagnosticBag diagnostics = new();
    private readonly Dictionary<VariableSymbol, object?> variables;

    public Binder(Dictionary<VariableSymbol, object?> variables)
    {
        this.variables = variables;
    }

    public IEnumerable<Diagnostic> Diagnostics => this.diagnostics;

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
        var expression = this.BindExpression(syntax.Expression);
        var name = syntax.IdentifierToken.Text;
        var existingVariable =  this.variables.Keys.FirstOrDefault(x => x.Name == name);
        if (existingVariable is not null)
        {
            this.variables.Remove(existingVariable);
        }
        
        var variable = new VariableSymbol(name, expression.Type);
        this.variables[variable] = null;
        
        return new BoundAssignmentExpression(variable, expression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var variable = this.variables.Keys.FirstOrDefault(x => x.Name == name);
        if (variable is null)
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