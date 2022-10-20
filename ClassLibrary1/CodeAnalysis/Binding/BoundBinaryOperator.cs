using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundBinaryOperator
{
    public BoundBinaryOperatorKind Kind { get; }
    public SyntaxKind SyntaxKind { get; }
    public TypeSymbol LeftType { get; }
    public TypeSymbol RightType { get; }
    public TypeSymbol ResultType { get; }

    BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol left) :
        this(kind, syntaxKind, left, left, left)
    {
    }

    BoundBinaryOperator(BoundBinaryOperatorKind kind, SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
    {
        Kind = kind;
        SyntaxKind = syntaxKind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    static readonly List<BoundBinaryOperator> _operators = new()
    {
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, TypeSymbol.Int),
        new(BoundBinaryOperatorKind.Addition, SyntaxKind.PlusToken, TypeSymbol.String),
        new(BoundBinaryOperatorKind.Subtraction, SyntaxKind.MinusToken, TypeSymbol.Int),
        new(BoundBinaryOperatorKind.Multiplication, SyntaxKind.StarToken, TypeSymbol.Int),
        new(BoundBinaryOperatorKind.Division, SyntaxKind.SlashToken, TypeSymbol.Int),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, TypeSymbol.Bool),
        
        new(BoundBinaryOperatorKind.BitwiseAnd, SyntaxKind.AmpersandToken, TypeSymbol.Int),
        new(BoundBinaryOperatorKind.BitwiseOr, SyntaxKind.PipeToken, TypeSymbol.Int),
        new(BoundBinaryOperatorKind.BitwiseXor, SyntaxKind.HatToken, TypeSymbol.Int),
        
        new(BoundBinaryOperatorKind.LogicalAnd, SyntaxKind.AmpersandAmpersandToken, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.LogicalOr, SyntaxKind.PipePipeToken, TypeSymbol.Bool),
        
        new(BoundBinaryOperatorKind.LessThan, SyntaxKind.LessToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.LessThanOrEquals, SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        
        new(BoundBinaryOperatorKind.GreaterThan, SyntaxKind.GreaterToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.GreaterThanOrEquals, SyntaxKind.GreaterOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.String),
        new(BoundBinaryOperatorKind.Equality, SyntaxKind.EqualsEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
        
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.Bool),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.String),
        new(BoundBinaryOperatorKind.Inequality, SyntaxKind.BangEqualsToken, TypeSymbol.Int, TypeSymbol.Int, TypeSymbol.Bool),
    };

    internal static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
    {
        foreach (var binaryOperator in _operators)
        {
            if (syntaxKind == binaryOperator.SyntaxKind && binaryOperator.LeftType == leftType && binaryOperator.RightType == rightType)
            {
                return binaryOperator;
            }
        }

        return null;
    }
    
    internal static BoundBinaryOperator? BindMethodCall(TypeSymbol type, FunctionSymbol function)
    {
        if (type.MethodTable.ContainsKey(function))
        {
            return new BoundBinaryOperator(BoundBinaryOperatorKind.MethodCall, SyntaxKind.DotToken, function.ReturnType);
        }

        return null;
    }
    
    internal static BoundBinaryOperator? BindFieldAccess(TypeSymbol type, FieldSymbol field)
    {
        if (type.FieldTable.Contains(field))
        {
            return new BoundBinaryOperator(BoundBinaryOperatorKind.FieldAccess, SyntaxKind.DotToken, field.Type);
        }

        return null;
    }
}