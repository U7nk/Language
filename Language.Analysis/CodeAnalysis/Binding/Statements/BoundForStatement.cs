using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundForStatement : BoundLoopStatement
{
    internal override BoundNodeKind Kind => BoundNodeKind.ForStatement;
    public BoundVariableDeclarationAssignmentStatement? VariableDeclaration { get; }
    public BoundExpression? Expression { get; }
    public BoundExpression Condition { get; }
    public BoundExpression Mutation { get; }
    public BoundStatement Body { get; }

    public BoundForStatement(BoundVariableDeclarationAssignmentStatement? variableDeclaration, BoundExpression? expression,
        BoundExpression condition, BoundExpression mutation, BoundStatement body,
        LabelSymbol breakLabel, LabelSymbol continueLabel)
        : base(breakLabel, continueLabel)
    {
        Condition = condition;
        Mutation = mutation;
        Body = body;
        VariableDeclaration = variableDeclaration;
        Expression = expression;
    }
}