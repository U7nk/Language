using FluentAssertions;
using FluentAssertions.Common;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;
using Xunit.Abstractions;
namespace TestProject1;

public class UnitTest1
{
    private void PrettyPrint(SyntaxNode node, bool isLast = true, string indent = "")
    {

        var marker = isLast ? "└──" : "├──";
        var str = indent + marker;
        str += node.Kind.ToString();

        if (node is SyntaxToken t && t.Value != null)
        {
            str += " ";
            str += t.Value;
        }

        this.output.WriteLine(str);

        indent += isLast ? "    " : "│   ";
        var last = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
        {
            this.PrettyPrint(child, child == last, indent);
        }
    }
    
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }
    
    [Fact]
    public void Custom()
    {
        this.output.WriteLine("Result: " + this.Build("(a = false) == (a)"));
    }

    private object Build(string input)
    {
        var syntaxTree = SyntaxTree.Parse(input);
        this.PrettyPrint(syntaxTree.Root);
        var variables = new Dictionary<VariableSymbol, object?>();
        var binder = new Binder(variables);
        var bindTree = binder.BindExpression(syntaxTree.Root);
        var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToList(); 
        if (diagnostics.Any())
        {
            foreach (var diagnostic in diagnostics)
            {
                var prefix = input.Substring(0, diagnostic.Span.Start);
                var error = input.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                var suffix = input.Substring(diagnostic.Span.End);
                this.output.WriteLine($"\"{prefix}~{error}~{suffix}\"");
                this.output.WriteLine(diagnostic.Message);
            }
        }
        else
        {
            var e = new Evaluator(bindTree, variables);
            var result = e.Evaluate();
            return result;
        }

        return diagnostics;
    }
}