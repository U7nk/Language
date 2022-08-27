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

    [Theory]
    [InlineData("1 + 2", 3)]
    [InlineData("1 - 2", -1)]
    [InlineData("1 * 2", 2)]
    [InlineData("4 / 2", 2)]
    [InlineData("1 + 2 * 3", 7)]
    [InlineData("false", false)]
    [InlineData("true", true)]
    [InlineData("true && false", false)]
    [InlineData("true || false", true)]
    [InlineData("false || false", false)]
    [InlineData("true && true", true)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("!false && !true", false)]
    [InlineData("!false && !false", true)]
    public void Test(string input, object expectedResult)
    {
        this.Build(input).Should().Be(expectedResult);
    }

    [Fact]
    public void Custom()
    {
        this.Build("2 | false");
    }

    private object Build(string input)
    {
        var syntaxTree = SyntaxTree.Parse(input);
        this.PrettyPrint(syntaxTree.Root);
        var binder = new Binder();
        var bindTree = binder.BindExpression(syntaxTree.Root);
        var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToList(); 
        if (diagnostics.Any())
        {
            foreach (var diagnostic in diagnostics)
            {
                this.output.WriteLine(diagnostic);
            }
        }
        else
        {
            var e = new Evaluator(bindTree);
            var result = e.Evaluate();
            return result;
        }

        return diagnostics;
    }
}