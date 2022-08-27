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
    
    public BoundUnaryOperator(BoundUnaryOperatorKind kind, SyntaxKind syntaxKind, Type operandType)
        : this(kind, syntaxKind, operandType, operandType)
    {
    }

    private static readonly List<BoundUnaryOperator> _operators = new()
    {
        new BoundUnaryOperator(BoundUnaryOperatorKind.Identity, SyntaxKind.PlusToken, typeof(int)),
        new BoundUnaryOperator(BoundUnaryOperatorKind.Negation, SyntaxKind.MinusToken, typeof(int)),

        new BoundUnaryOperator(BoundUnaryOperatorKind.LogicalNegation, SyntaxKind.BangToken, typeof(bool)),
    };

    internal static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, Type operandType)
    {
        foreach (var unaryOperator in _operators)
        {
            if (syntaxKind == unaryOperator.SyntaxKind && unaryOperator.OperandType == operandType)
            {
                return unaryOperator;
            }
        }

        return null;
    }
}