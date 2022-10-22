using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Language.CodeAnalysis.Binding;

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