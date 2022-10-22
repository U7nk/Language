namespace Language.CodeAnalysis.Binding;

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
    BitwiseXor,
    
    MethodCall,
    FieldAccess,
}