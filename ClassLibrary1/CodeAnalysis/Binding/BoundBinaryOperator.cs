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

    private static readonly List<BoundBinaryOperator> _operators = new()
    {
        new BoundBinaryOperator(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, typeof(int)),
        new BoundBinaryOperator(BoundBinaryOperatorKind.Subtraction, SyntaxKind.MinusToken, typeof(int)),
        new BoundBinaryOperator(BoundBinaryOperatorKind.Multiplication, SyntaxKind.StarToken, typeof(int)),
        new BoundBinaryOperator(BoundBinaryOperatorKind.Division, SyntaxKind.SlashToken, typeof(int)),
        
        new BoundBinaryOperator(BoundBinaryOperatorKind.LogicalAnd, SyntaxKind.AmpersandAmpersandToken, typeof(bool)),
        new BoundBinaryOperator(BoundBinaryOperatorKind.LogicalOr, SyntaxKind.PipePipeToken, typeof(bool)),
    };

    internal static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
    {
        foreach (var unaryOperator in _operators)
        {
            if (syntaxKind == unaryOperator.SyntaxKind && unaryOperator.LeftType == leftType && unaryOperator.RightType == rightType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}