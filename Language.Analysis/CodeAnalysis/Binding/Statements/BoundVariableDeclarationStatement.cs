using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundVariableDeclarationStatement : BoundStatement
{
    public BoundVariableDeclarationStatement(SyntaxNode syntax, VariableSymbol variable) : base(syntax)
    {
        Variable = variable;
    }

    public VariableSymbol Variable { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
}