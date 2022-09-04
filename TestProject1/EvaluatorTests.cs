using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;

namespace TestProject1;

using FluentAssertions;
using Wired.CodeAnalysis.Syntax;

public class EvaluatorTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("5 * 2", 10)]
    [InlineData("2 + 5 * 2", 12)]
    [InlineData("(2 + 5) * 2", 14)]
    [InlineData("+1", 1)]
    [InlineData("-1", -1)]
    [InlineData("-1 + 2", 1)]
    [InlineData("-(1 + 2)", -3)]
    [InlineData("1 - 2", -1)]
    [InlineData("9 / 3", 3)]
    [InlineData("(9)", 9)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!false", true)]
    [InlineData("!true", false)]
    [InlineData("!!true", true)]
    [InlineData("!!false", false)]
    [InlineData("false || true", true)]
    [InlineData("false && true", false)]
    [InlineData("false == true", false)]
    [InlineData("true == false", false)]
    [InlineData("true == true", true)]
    [InlineData("true != true", false)]
    [InlineData("true != false", true)]
    [InlineData("false != true", true)]
    [InlineData("12 == 1", false)]
    [InlineData("12 == 12", true)]
    [InlineData("12 != 12", false)]
    [InlineData("12 != 1", true)]
    [InlineData("(a = 10) * a", 100)]
    public void Evaluator_Evaluates(string expression, object expectedValue)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        syntaxTree.Diagnostics.Should().BeEmpty();
        
        var variables = new Dictionary<VariableSymbol, object?>();
        var binder = new Binder(variables);
        var typeChecked = binder.BindExpression(syntaxTree.Root);
        binder.Diagnostics.ToList().Should().BeEmpty();
        
        var evaluator = new Evaluator(typeChecked, variables);
        var actualValue = evaluator.Evaluate();
        actualValue.Should().Be(expectedValue);
    }
}