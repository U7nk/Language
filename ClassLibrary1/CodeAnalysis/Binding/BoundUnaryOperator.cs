using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundUnaryOperator
{
    public BoundUnaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public TypeSymbol OperandType { get; }
    public TypeSymbol ResultType { get; }

    BoundUnaryOperator(BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol operandType, TypeSymbol resultType)
    {
        Kind = kind;
        SyntaxKind = syntaxKind;
        OperandType = operandType;
        ResultType = resultType;
    }
    
    public BoundUnaryOperator(
        BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol operandType) 
        : this(kind, syntaxKind, operandType, operandType)
    {
    }

    static readonly List<BoundUnaryOperator> _operators = new()
    {
        new(BoundUnaryOperatorKind.Identity, SyntaxKind.PlusToken, TypeSymbol.Int),
        new(BoundUnaryOperatorKind.Negation, SyntaxKind.MinusToken, TypeSymbol.Int),

        new(BoundUnaryOperatorKind.LogicalNegation, SyntaxKind.BangToken, TypeSymbol.Bool),
        new(BoundUnaryOperatorKind.BitwiseNegation, SyntaxKind.TildeToken, TypeSymbol.Int),
    };

    internal static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
    {
        foreach (var unaryOperator in _operators)
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