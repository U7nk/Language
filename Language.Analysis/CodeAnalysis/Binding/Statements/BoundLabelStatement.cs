using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(LabelSymbol label)
    {
        Label = label;
    }

    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
}