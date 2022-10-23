using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundVariableDeclarationStatement : BoundStatement
{
    public VariableSymbol Variable { get; }
    public BoundExpression Initializer { get; }

    public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initializer)
    {
        Variable = variable;
        Initializer = initializer;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
}