using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wired.CodeAnalysis.Syntax;

public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span
    {
        get
        {
            var first = this.GetChildren().First().Span;
            var last = this.GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public IEnumerable<SyntaxNode> GetChildren()
    {
        var properties = this.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.PropertyType.CanBeConvertedTo<SyntaxNode>())
            {
                yield return (SyntaxNode)(property.GetValue(this) ?? throw new InvalidOperationException());
            }
            else if (property.PropertyType.CanBeConvertedTo<IEnumerable<SyntaxNode>>())
            {
                var children = (IEnumerable<SyntaxNode>)(property.GetValue(this) ?? throw new InvalidOperationException());
                foreach (var child in children)
                {
                    yield return child;
                }
            }
        }
    }
    
    public void WriteTo(TextWriter writer)
    {
        this.PrettyPrint(writer, this);
    }
    
    private void PrettyPrint(TextWriter writer, SyntaxNode node, bool isLast = true, string indent = "")
    {

        var marker = isLast ? "└──" : "├──";
        var str = indent + marker;
        str += node.Kind.ToString();

        if (node is SyntaxToken { Value: { } } t)
        {
            str += " ";
            str += t.Value;
        }

        writer.WriteLine(str);

        indent += isLast ? "    " : "│   ";
        var last = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
        {
            this.PrettyPrint(writer, child, child == last, indent);
        }
    }
}