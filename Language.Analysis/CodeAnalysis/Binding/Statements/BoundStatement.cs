using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

public abstract class BoundStatement : BoundNode
{
    protected BoundStatement(Option<SyntaxNode> syntax) : base(syntax)
    {
    }
}