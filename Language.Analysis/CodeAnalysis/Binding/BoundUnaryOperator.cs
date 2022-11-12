using System.Collections.Generic;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundUnaryOperator
{
    public BoundUnaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public TypeSymbol OperandType { get; }
    public TypeSymbol ResultType { get; }

    BoundUnaryOperator(
        BoundUnaryOperatorKind kind, SyntaxKind syntaxKind,
        TypeSymbol operandType, TypeSymbol resultType)
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
        new(BoundUnaryOperatorKind.Identity, SyntaxKind.PlusToken, BuiltInTypeSymbols.Int),
        new(BoundUnaryOperatorKind.Negation, SyntaxKind.MinusToken, BuiltInTypeSymbols.Int),

        new(BoundUnaryOperatorKind.LogicalNegation, SyntaxKind.BangToken, BuiltInTypeSymbols.Bool),
        new(BoundUnaryOperatorKind.BitwiseNegation, SyntaxKind.TildeToken, BuiltInTypeSymbols.Int),
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