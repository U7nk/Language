using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal abstract class BoundNode
{
    internal abstract BoundNodeKind Kind { get; }
    
    public IEnumerable<BoundNode> GetChildren()
    {
        var properties = this.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var property in properties)
        {
            if (property.PropertyType.CanBeConvertedTo<BoundNode>())
            {
                var value = property.GetValue(this); 
                if (value != null)
                    yield return (BoundNode)value;
            }
            else if (property.PropertyType.CanBeConvertedTo<IEnumerable<BoundNode>>())
            {
                var children = (IEnumerable<BoundNode>)(property.GetValue(this) ?? throw new InvalidOperationException());
                foreach (var child in children)
                {
                    yield return child;
                }
            }
        }
    }

    public static string GetPropertiesText(BoundNode node)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var property in node.GetProperties())
        {
            if (first)
                first = false;
            else
                sb.Append(", ");

            sb.Append(property.Name);
            sb.Append(" = ");
            sb.Append(property.Value);
        }

        return sb.ToString();
    } 
    
    public IEnumerable<(string Name, object Value)> GetProperties()
    {
        var properties = this.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var property in properties)
        {
            if (property.Name is 
                nameof(BoundNode.Kind)
                or nameof(BoundBinaryExpression.Op)) 
                continue;
            
            if (property.PropertyType.CanBeConvertedTo<BoundNode>() ||
                property.PropertyType.CanBeConvertedTo<IEnumerable<BoundNode>>())
                continue;
            
            var value = property.GetValue(this);
            if (value != null)
                yield return (property.Name, value);

        }
    }
    
    private void PrettyPrint(TextWriter writer, BoundNode node, bool isLast = true, string indent = "")
    {
        var marker = isLast ? "└──" : "├──";
        var str = indent + marker;
        str += GetText(node) + " " + GetPropertiesText(node);
        
        writer.WriteLine(str);
        
        indent += isLast ? "    " : "│   ";
        var last = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
        {
            this.PrettyPrint(writer, child, child == last, indent);
        }
    }

    private static string GetText(BoundNode node)
    {
        if (node is BoundBinaryExpression b)
            return b.Op.Kind + "Expression";

        if (node is BoundUnaryExpression u)
            return u.Op.Kind + "Expression";

        return node.Kind.ToString();
    }

    public void WriteTo(TextWriter writer) 
        => this.PrettyPrint(writer, this);
}