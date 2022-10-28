using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundVariableDeclarationStatement : BoundStatement
{
    public BoundVariableDeclarationStatement(VariableSymbol variable)
    {
        Variable = variable;
    }

    public VariableSymbol Variable { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
}