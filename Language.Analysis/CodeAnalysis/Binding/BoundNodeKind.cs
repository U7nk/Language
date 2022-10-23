namespace Language.Analysis.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // statements 
    BlockStatement,
    ExpressionStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    GotoStatement,
    LabelStatement,
    ConditionalGotoStatement,
    ReturnStatement,

    // expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    ErrorExpression,
    MethodCallExpression,
    ConversionExpression,
    ThisExpression,
    ObjectCreationExpression,
    FieldExpression,
    FieldAssignmentExpression
}