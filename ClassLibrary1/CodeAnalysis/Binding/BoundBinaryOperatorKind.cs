namespace Wired.CodeAnalysis.Binding;

internal enum BoundBinaryOperatorKind
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    LogicalOr,
    LogicalAnd,
    Equality,
    Inequality,
    LessThan,
    GreaterThan,
    GreaterThanOrEquals,
    LessThanOrEquals,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor
}