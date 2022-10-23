using System.IO;

namespace Language.Analysis.CodeAnalysis.Binding;

public abstract class BoundNode
{
    internal abstract BoundNodeKind Kind { get; }

    public override string ToString()
    {
        using var writer = new StringWriter();
        this.WriteTo(writer);
        return writer.ToString();
    }
}