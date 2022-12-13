using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundForStatement : BoundLoopStatement
{
    internal override BoundNodeKind Kind => BoundNodeKind.ForStatement;
    public BoundVariableDeclarationAssignmentStatement? VariableDeclarationAssignment { get; }
    public BoundExpression? Expression { get; }
    public BoundExpression Condition { get; }
    public BoundExpression Mutation { get; }
    public BoundStatement Body { get; }

    public BoundForStatement(Option<SyntaxNode> syntax, BoundVariableDeclarationAssignmentStatement? variableDeclarationAssignment,
                             BoundExpression? expression,
                             BoundExpression condition, BoundExpression mutation, BoundStatement body,
                             LabelSymbol breakLabel, LabelSymbol continueLabel)
        : base(syntax, breakLabel, continueLabel)
    {
        Condition = condition;
        Mutation = mutation;
        Body = body;
        VariableDeclarationAssignment = variableDeclarationAssignment;
        Expression = expression;
    }
}