using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryOperator
{
    public BoundUnaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public TypeSymbol OperandType { get; }
    public TypeSymbol ResultType { get; }
    
    private BoundUnaryOperator(BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol operandType, TypeSymbol resultType)
    {
        this.Kind = kind;
        this.SyntaxKind = syntaxKind;
        this.OperandType = operandType;
        this.ResultType = resultType;
    }
    
    public BoundUnaryOperator(
        BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol operandType) 
        : this(kind, syntaxKind, operandType, operandType)
    {
    }

    private static readonly List<BoundUnaryOperator> Operators = new()
    {
        new(BoundUnaryOperatorKind.Identity, SyntaxKind.PlusToken, TypeSymbol.Int),
        new(BoundUnaryOperatorKind.Negation, SyntaxKind.MinusToken, TypeSymbol.Int),

        new(BoundUnaryOperatorKind.LogicalNegation, SyntaxKind.BangToken, TypeSymbol.Bool),
        new(BoundUnaryOperatorKind.BitwiseNegation, SyntaxKind.TildeToken, TypeSymbol.Int),
    };

    internal static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
    {
        foreach (var unaryOperator in Operators)
        {
            if (syntaxKind == unaryOperator.SyntaxKind 
                && unaryOperator.OperandType == operandType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}