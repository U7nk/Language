namespace Wired.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // statements 
    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    
    // expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    
}