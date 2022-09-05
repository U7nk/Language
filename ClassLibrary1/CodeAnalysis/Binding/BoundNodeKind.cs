namespace Wired.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // statements 
    BlockStatement,
    
    // expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    ExpressionStatement
}