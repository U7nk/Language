using FluentAssertions;
using FluentAssertions.Common;
using Wired.CodeAnalysis;
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
    public void Test()
    {
        const string input = "(2 + 1) * 2";
        var syntaxTree = SyntaxTree.Parse(input);
        this.PrettyPrint(syntaxTree.Root);
        if (syntaxTree.Diagnostics.Any())
        {
            foreach (var diagnostic in syntaxTree.Diagnostics)
            {
                this.output.WriteLine(diagnostic);   
            }
        }
        else
        {
            var e = new Evaluator(syntaxTree.Root);
            var result = e.Evaluate();
            this.output.WriteLine("Result: ");
            this.output.WriteLine(result.ToString());
        }
        
    }
}