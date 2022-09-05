using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;
using Xunit.Abstractions;

namespace TestProject1;

using FluentAssertions;
using Wired.CodeAnalysis.Syntax;

public class EvaluatorTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public EvaluatorTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("1;", 1)]
    [InlineData("5 * 2;", 10)]
    [InlineData("2 + 5 * 2;", 12)]
    [InlineData("(2 + 5) * 2;", 14)]
    [InlineData("+1;", 1)]
    [InlineData("-1;", -1)]
    [InlineData("-1 + 2;", 1)]
    [InlineData("-(1 + 2);", -3)]
    [InlineData("1 - 2;", -1)]
    [InlineData("9 / 3;", 3)]
    [InlineData("(9);", 9)]
    [InlineData("true;", true)]
    [InlineData("false;", false)]
    [InlineData("!false;", true)]
    [InlineData("!true;", false)]
    [InlineData("!!true;", true)]
    [InlineData("!!false;", false)]
    [InlineData("false || true;", true)]
    [InlineData("false && true;", false)]
    [InlineData("false == true;", false)]
    [InlineData("true == false;", false)]
    [InlineData("true == true;", true)]
    [InlineData("true != true;", false)]
    [InlineData("true != false;", true)]
    [InlineData("false != true;", true)]
    [InlineData("12 == 1;", false)]
    [InlineData("12 == 12;", true)]
    [InlineData("12 != 12;", false)]
    [InlineData("12 != 1;", true)]
    [InlineData("{ let a = 10; a * a; }", 100)]
    [InlineData("{ let a = 10; a = 7; }", null, true)]
    [InlineData("{ var a = 10; a * a; }", 100)]
    [InlineData("{ var a = 10; a = 5; }", 5)]
    [InlineData("{ var a = 10; { let a = false; } a = 5; }", 5)]
    [InlineData("{ var a = 10; { var a = 50; } a = 5; }", 5)]
    [InlineData("{ var a = 10; { var b = 50; } var b = false; b; }", false)]
    public void Evaluator_Evaluates(string expression, object expectedValue, bool error = false)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        syntaxTree.Diagnostics.Should().BeEmpty();
        
        var compilation = new Compilation(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object?>();
        var evaluation = compilation.Evaluate(variables);
        if (!error)
        {
            evaluation.Diagnostics.ToList().Should().BeEmpty();
            evaluation.Result.Should().Be(expectedValue);
        }
        else
        {
            evaluation.Diagnostics.ToList().Should().NotBeEmpty();
            foreach (var diagnostic in evaluation.Diagnostics)
            {
                testOutputHelper.WriteLine(diagnostic.Message);
            }
        }
    }
}