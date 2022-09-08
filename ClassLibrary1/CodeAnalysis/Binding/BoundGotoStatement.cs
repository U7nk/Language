namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundGotoStatement : BoundStatement
{
    public BoundGotoStatement(LabelSymbol label)
    {
        this.Label = label;
    }

    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
}