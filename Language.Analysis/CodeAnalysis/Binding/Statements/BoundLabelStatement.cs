using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(SyntaxNode? syntax, LabelSymbol label) : base(syntax)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
}