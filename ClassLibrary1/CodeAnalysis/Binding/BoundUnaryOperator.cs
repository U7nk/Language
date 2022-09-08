using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryOperator
{
    public BoundUnaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public Type OperandType { get; }
    public Type ResultType { get; }
    
    private BoundUnaryOperator(BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, Type operandType, Type resultType)
    {
        this.Kind = kind;
        this.SyntaxKind = syntaxKind;
        this.OperandType = operandType;
        this.ResultType = resultType;
    }
    
    public BoundUnaryOperator(
        BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, Type operandType) 
        : this(kind, syntaxKind, operandType, operandType)
    {
    }

    private static readonly List<BoundUnaryOperator> Operators = new()
    {
        new(BoundUnaryOperatorKind.Identity, SyntaxKind.PlusToken, typeof(int)),
        new(BoundUnaryOperatorKind.Negation, SyntaxKind.MinusToken, typeof(int)),

        new(BoundUnaryOperatorKind.LogicalNegation, SyntaxKind.BangToken, typeof(bool)),
        new(BoundUnaryOperatorKind.BitwiseNegation, SyntaxKind.TildeToken, typeof(int)),
    };

    internal static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, Type operandType)
    {
        foreach (var unaryOperator in Operators)
        {
            if (syntaxKind == unaryOperator.SyntaxKind && unaryOperator.OperandType == operandType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}