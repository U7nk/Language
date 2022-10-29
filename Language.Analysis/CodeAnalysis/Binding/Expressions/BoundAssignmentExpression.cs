using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundAssignmentExpression : BoundExpression
{
    public VariableSymbol Variable { get; }
    public BoundExpression Expression { get; }

    public BoundAssignmentExpression(SyntaxNode? syntax, VariableSymbol variable, BoundExpression expression) : base(syntax)
    {
        Variable = variable;
        Expression = expression;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    internal override TypeSymbol Type => Expression.Type;
}