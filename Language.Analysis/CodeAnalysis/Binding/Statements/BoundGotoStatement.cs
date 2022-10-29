using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundGotoStatement : BoundStatement
{
    public BoundGotoStatement(SyntaxNode? syntax, LabelSymbol label) : base(syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
}