using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding;

public abstract class BoundNode
{
    protected BoundNode(Option<SyntaxNode> syntax)
    {
        Syntax = syntax;
    }
    
    public Option<SyntaxNode> Syntax { get; }
    internal abstract BoundNodeKind Kind { get; }

    internal IEnumerable<BoundNode> GetChildren(bool recursion = false)
    {
        yield return this;
        
        var properties = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.PropertyType.CanBeConvertedTo<BoundNode>())
            {
                var value = property.GetValue(this);
                if (value == null)
                {
                    continue;
                }
                
                if (recursion)
                {
                    foreach (var child in ((BoundNode)value).GetChildren(true))
                    {
                        yield return child;
                    }
                }
                else
                {
                    yield return (BoundNode)value;
                }
            }
            else if (property.PropertyType.CanBeConvertedTo<IEnumerable<BoundNode>>())
            {
                
                var value = ((IEnumerable<BoundNode>?)property.GetValue(this))
                    .NullGuard()
                    .ToList();

                foreach (var child in value)
                {
                    if (recursion)
                    {
                        foreach (var grandChild in child.GetChildren(true))
                            yield return grandChild;
                    }
                    else
                    {
                        yield return child;
                    }
                }

            }
        }
    }

    public override string ToString()
    {
        using var writer = new StringWriter();
        this.WriteTo(writer);
        return writer.ToString();
    }
}