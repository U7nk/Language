using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Syntax;

public interface ISyntaxNode
{
    public SyntaxTree SyntaxTree { get; }
    public abstract SyntaxKind Kind { get; }
}

public abstract class SyntaxNode : ISyntaxNode
{
    protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }
    
    public SyntaxTree SyntaxTree { get; }
    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span
    {
        get
        {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }

    public TextLocation Location => new(SyntaxTree.SourceText, Span);
    public IEnumerable<SyntaxNode> GetChildren()
    {
        var properties = GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var property in properties)
        {
            object? GetValue(PropertyInfo property)
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Option<>))
                {
                    var value = property.GetValue(this);
                    return property.PropertyType.GetMethod(nameof(Option<Unit>.Unwrap))
                        .NullGuard()
                        .Invoke(value, Array.Empty<object>())
                        .NullGuard();
                }

                return property.GetValue(this);
            }
            
            var propertyType = property.PropertyType;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Option<>))
            {
                var isSome = (bool)propertyType.GetProperty(nameof(Option<Unit>.IsSome))
                    .NullGuard()
                    .GetValue(property.GetValue(this))
                    .NullGuard();
                if (isSome)
                {
                    var value = GetValue(property);
                    propertyType = value.NullGuard().GetType();
                }
            }
            
            if (propertyType.CanBeConvertedTo<SyntaxNode>())
            {
                var value = GetValue(property);
                if (value != null)
                    yield return (SyntaxNode)value;
            }
            else if (propertyType.CanBeConvertedTo<SeparatedSyntaxList>())
            {
                var value = (SeparatedSyntaxList)property.GetValue(this).NullGuard();

                foreach (var child in value.GetWithSeparators())
                    yield return child;

            }
            else if (propertyType.CanBeConvertedTo<IEnumerable<SyntaxNode>>())
            {
                var children = (IEnumerable<SyntaxNode>)GetValue(property).NullGuard();
                foreach (var child in children)
                {
                    yield return child;
                }
            }
        }
    }

    public SyntaxToken GetLastToken()
    {
        if (this is SyntaxToken syntaxToken)
            return syntaxToken;
        
        // fact: syntax node always have at least one token
        return GetChildren().Last().GetLastToken();
    }

    public override string ToString()
    {
        using var sw = new StringWriter();
        WriteTo(sw);
        return sw.ToString();
    }

    public void WriteTo(TextWriter writer)
    {
        PrettyPrint(writer, this);
    }


    void PrettyPrint(TextWriter writer, SyntaxNode node, bool isLast = true, string indent = "")
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
            PrettyPrint(writer, child, child == last, indent);
        }
    }
}