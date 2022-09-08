using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryOperator
{
    public BoundBinaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public Type LeftType { get; }
    public Type RightType { get; }
    public Type ResultType { get; }

    private BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, Type left) :
        this(kind, syntaxKind, left, left, left)
    {
    }

    private BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, Type leftType, Type rightType, Type resultType)
    {
        this.Kind = kind;
        this.SyntaxKind = syntaxKind;
        this.LeftType = leftType;
        this.RightType = rightType;
        this.ResultType = resultType;
    }

    private static readonly List<BoundBinaryOperator> Operators = new()
    {
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, typeof(int)),
        new(BoundBinaryOperatorKind.Subtraction, SyntaxKind.MinusToken, typeof(int)),
        new(BoundBinaryOperatorKind.Multiplication, SyntaxKind.StarToken, typeof(int)),
        new(BoundBinaryOperatorKind.Division, SyntaxKind.SlashToken, typeof(int)),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, typeof(bool)),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, typeof(bool)),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, typeof(bool)),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, typeof(int)),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, typeof(int)),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, typeof(int)),
        
        new(BoundBinaryOperatorKind.LogicalAnd, SyntaxKind.AmpersandAmpersandToken, typeof(bool)),
        new(BoundBinaryOperatorKind.LogicalOr, SyntaxKind.PipePipeToken, typeof(bool)),
        
        new(BoundBinaryOperatorKind.LessThan, SyntaxKind.LessToken, typeof(int), typeof(int), typeof(bool)),
        new(BoundBinaryOperatorKind.LessThanOrEquals, SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int), typeof(bool)),
        
        new(BoundBinaryOperatorKind.GreaterThan, SyntaxKind.GreaterToken, typeof(int), typeof(int), typeof(bool)),
        new(BoundBinaryOperatorKind.GreaterThanOrEquals, SyntaxKind.GreaterOrEqualsToken, typeof(int), typeof(int), typeof(bool)),
        
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, typeof(bool)),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, typeof(int), typeof(int), typeof(bool)),
        
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, typeof(bool)),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, typeof(int), typeof(int), typeof(bool)),
    };

    internal static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
    {
        foreach (var unaryOperator in Operators)
        {
            if (syntaxKind == unaryOperator.SyntaxKind && unaryOperator.LeftType == leftType && unaryOperator.RightType == rightType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}