using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly List<string> diagnostics = new();
    internal IEnumerable<string> Diagnostics => this.diagnostics;
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
            default:
                throw new Exception($"Unexpected syntax {syntax.Kind}");
        }
    }

    private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind syntaxKind, Type operandType)
    {
        if (operandType == typeof(int))
        {
            return syntaxKind switch
            {
                SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
                SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
                _ => null,
            };
        }

        if (operandType == typeof(bool))
        {
            return syntaxKind switch
            {
                SyntaxKind.BangToken => BoundUnaryOperatorKind.LogicalNegation,
                _ => null
            };
        }

        return null;
    }

    private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind syntaxKind, Type leftType, Type rightType)
    {
        if (leftType == typeof(int) && rightType == typeof(int))
        {
            return syntaxKind switch
            {
                SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
                SyntaxKind.MinusToken => BoundBinaryOperatorKind.Subtraction,
                SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
                SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
                _ => null,
            };
        }

        if (leftType == typeof(bool) && rightType == typeof(bool))
        {
            return syntaxKind switch
            {
                SyntaxKind.AmpersandAmpersandToken => BoundBinaryOperatorKind.LogicalAnd,
                SyntaxKind.PipePipeToken => BoundBinaryOperatorKind.LogicalOr,
                _ => null,
            };
        }

        return null;
    }
    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var operand = this.BindExpression(syntax.Operand);
        var unaryOperator = this.BindUnaryOperatorKind(syntax.OperatorToken.Kind, operand.Type);
        if (unaryOperator is null)
        {
            this.diagnostics.Add($"Unary operator \'{syntax.OperatorToken.Text}\' not defined for type \'{operand.Type}\'.");
            return operand;
        }
        return new BoundUnaryExpression(unaryOperator.Value, operand);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var left = this.BindExpression(syntax.Left);
        var right = this.BindExpression(syntax.Right);
        var binaryOperator = this.BindBinaryOperatorKind(syntax.OperatorToken.Kind, left.Type, right.Type);
        if (binaryOperator is null)
        {
            this.diagnostics.Add($"Binary operator \'{syntax.OperatorToken.Text}\' not defined for types \'{left.Type}\' and \'{right.Type}\'.");
            return left;
        }
        return new BoundBinaryExpression(left, binaryOperator.Value, right);
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