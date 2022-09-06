using System.Collections.Immutable;
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
    [InlineData("12 > 1;", true)]
    [InlineData("12 < 1;", false)]
    [InlineData("4 >=4;", true)]
    [InlineData("!(4 >= 5);", true)]
    [InlineData("!(4 > 5);", true)]
    [InlineData("!(4 < 5);", false)]
    [InlineData("!(4 <= 5);", false)]
    [InlineData("4 <= 4;", true)]
    [InlineData("4 < 4;", false)]
    [InlineData("3 < 4;", true)]
    [InlineData("3 > 4;", false)]
    [InlineData("5 > 4;", true)]
    [InlineData("{ let a = 10; a * a; }", 100)]
    [InlineData("{ var a = 10; a * a; }", 100)]
    [InlineData("{ var a = 10; a = 5; }", 5)]
    [InlineData("{ var a = 10; { let a = false; } a = 5; }", 5)]
    [InlineData("{ var a = 10; { var a = 50; } a = 5; }", 5)]
    [InlineData("{ var a = 10; { var b = 50; } var b = false; b; }", false)]
    [InlineData("{ var a = 10; if true == true a = 2; a; }", 2)]
    public void Evaluator_Evaluates(string expression, object expectedValue)
    {
        AssertValue(expression, expectedValue);
    }

    private static void AssertValue(string expression, object expectedValue)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        syntaxTree.Diagnostics.Should().BeEmpty();

        var compilation = new Compilation(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object?>();
        var evaluation = compilation.Evaluate(variables);

        evaluation.Diagnostics.ToList().Should().BeEmpty();
        evaluation.Result.Should().Be(expectedValue);
    }

    [Fact]
    public void Evaluator_VariableDeclaration_Reports_Redeclaration()
    {
        var text = 
            $$"""
            {
                var a = 10;
                [var a] = 10;
            } 
            """;
        var diagnostics = new[] {
            "Variable 'a' is already declared.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_NameExpression_Reports_UndefinedVariable()
    {
        var text = 
            $$"""
            {
                var a = [b];
            } 
            """;
        var diagnostics = new[] {
            "'b' is undefined.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_AssignedExpression_Reports_CannotAssignVariable()
    {
        var text = 
            $$"""
            {
                let a = 10;
                [a =] 50;
            } 
            """;
        var diagnostics = new[] {
            "'a' is readonly and cannot be assigned to.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_AssignedExpression_Reports_CannotConvertVariable()
    {
        var text = 
            $$"""
            {
                var a = 10;
                a = [false];
            } 
            """;
        var diagnostics = new[] {
            "Cannot convert 'System.Int32' to 'System.Boolean'.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    private static void AssertDiagnostics(string text, string[] diagnosticsText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        diagnostics.Length.Should()
            .Be(diagnosticsText.Length)
            .And.Be(annotatedText.Spans.Length);
        
        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        for (int i = 0; i < diagnostics.Length; i++)
        {
            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].Span;
            actualSpan.Should().Be(expectedSpan, "Diagnostic spans do not match");

            var expectedMessage = diagnosticsText[i];
            var actualMessage = diagnostics[i].Message;
            actualMessage.Should().Be(expectedMessage, "Diagnostic messages do not match");
        }
    }
}