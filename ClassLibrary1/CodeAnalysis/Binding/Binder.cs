using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly DiagnosticBag diagnostics = new();
    internal IEnumerable<Diagnostic> Diagnostics => this.diagnostics;
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
                return this.BindExpression((ParenthesizedExpressionSyntax)syntax);
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
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
        if (value is null)
        {
            throw new Exception($"Literal expression value is null. Kind: {syntax.Kind}");
        }
        return new BoundLiteralExpression(value);
    }
}