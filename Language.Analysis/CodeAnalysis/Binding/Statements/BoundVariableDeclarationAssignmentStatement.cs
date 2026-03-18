using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundVariableDeclarationAssignmentStatement : BoundVariableDeclarationStatement
{
    
    public BoundExpression Initializer { get; }

    public BoundVariableDeclarationAssignmentStatement(Option<SyntaxNode> syntaxNode, VariableSymbol variable, BoundExpression initializer) 
        : base(syntaxNode, variable)
    {
        Initializer = initializer;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationAssignmentStatement;
}