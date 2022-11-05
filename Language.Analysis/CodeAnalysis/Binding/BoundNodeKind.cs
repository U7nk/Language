namespace Language.Analysis.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // statements 
    BlockStatement,
    ExpressionStatement,
    VariableDeclarationAssignmentStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    GotoStatement,
    LabelStatement,
    ConditionalGotoStatement,
    ReturnStatement,
    
    #region expressions
    
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    /// <summary>
    /// variable access expression
    /// </summary>
    VariableExpression,
    AssignmentExpression,
    ErrorExpression,
    MethodCallExpression,
    ConversionExpression,
    ThisExpression,
    ObjectCreationExpression,
    MemberAccessExpression,
    MemberAssignmentExpression,
    FieldExpression,
    NamedTypeExpression,
    
    #endregion
     
}