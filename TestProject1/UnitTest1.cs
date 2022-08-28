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
    [InlineData("!false == !false", true)]
    [InlineData("!false != !false", false)]
    [InlineData("false == !false", false)]
    [InlineData("false == !false == true", false)]
    [InlineData("false == !false == false", true)]
    [InlineData("false || !false == false", false)]
    [InlineData("15 + 11 == 26", true)]
    [InlineData("15 * 2 + 2 == 32", true)]
    [InlineData("15 * 2 + 2 != 42", true)]
    [InlineData("15 * 2 + 2 == 32 && true == true", true)]
    [InlineData("1 == 1 && true", true)]
    [InlineData("a = 1", 1)]
    [InlineData("a = b = 1", 1)]
    [InlineData("(a = b = 1) == b == true", true)]
    [InlineData("(a = b = 1) == b && false", false)]
    public void Test(string input, object expectedResult)
    {
        this.Build(input).Should().Be(expectedResult);
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