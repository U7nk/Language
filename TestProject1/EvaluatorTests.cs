using System.Collections.Immutable;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;
using Xunit.Abstractions;

namespace TestProject1;

using FluentAssertions;
using Wired.CodeAnalysis.Syntax;

public class EvaluatorTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public EvaluatorTests(ITestOutputHelper testOutputHelper)
    {
        this._testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("1;", 1)]
    [InlineData("(9);", 9)]
    [InlineData("5 * 2;", 10)]
    [InlineData("2 + 5 * 2;", 12)]
    [InlineData("(2 + 5) * 2;", 14)]
    [InlineData("+1;", 1)]
    [InlineData("-1;", -1)]
    [InlineData("~1;", -2)]
    [InlineData("~6;", -7)]
    [InlineData("~-7;", 6)]
    [InlineData("-~7;", 8)]
    [InlineData("-1 + 2;", 1)]
    [InlineData("-(1 + 2);", -3)]
    [InlineData("1 - 2;", -1)]
    [InlineData("9 / 3;", 3)]
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
    [InlineData("false | false;", false)]
    [InlineData("false | true;", true)]
    [InlineData("true | true;", true)]
    [InlineData("true | false;", true)]
    [InlineData("false & false;", false)]
    [InlineData("false & true;", false)]
    [InlineData("true & true;", true)]
    [InlineData("true & false;", false)]
    [InlineData("false ^ false;", false)]
    [InlineData("false ^ true;", true)]
    [InlineData("true ^ true;", false)]
    [InlineData("true & false;", false)]
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
    [InlineData("1 | 2;", 3)]
    [InlineData("1 & 2;", 0)]
    [InlineData("1 & 3;", 1)]
    [InlineData("1 ^ 2;", 3)]
    [InlineData("1 ^ 3;", 2)]
    [InlineData("{ let a = 10; a * a; }", 100)]
    [InlineData("{ var a = 10; a * a; }", 100)]
    [InlineData("{ var a = 10; a = 5; }", 5)]
    [InlineData("{ var a = 10; if true == true a = 2; a; }", 2)]
    [InlineData("{ var a = 0; while a < 5 a = a + 1; a;}", 5)]
    [InlineData("{ let hello = \"hello\"; hello;}", "hello")]
    [InlineData(
        $$"""
        {   
            var x = 10; 
            if true != false
            {
                x = 5;
            }
            x;
        }
      """, 5)]
    [InlineData(
        $$"""
        {   
            var x = 10; 
            if true == false
            {
                x = 5;
            }
            x;
        }
      """, 10)]
    [InlineData(
      $$"""
       { 
         var a = 0; 
         var b = 1;
         while a < 5 
         { 
           a = a + 1;
           b = b + 1; 
         }
         a + b;
       }
      """, 11)]
    [InlineData(
        $$"""
       {  
         var b = 1; 
         var i = 5; 
         for (i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
       }
      """, 3)]
    [InlineData(
        $$"""
       {  
         var b = 1; 
         for (var i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
       }
      """, 3)]
    [InlineData(
        $$"""
        {
            var result = 0;
            if 1 > 1
                result = 1;
            else 
                result = 2;
            result;
        }
        """, 2)]
    [InlineData(
        $$"""
        {
            var result = 0;
            result;
        }
        """, 0)]
    [InlineData(
        $$"""
        {
            var result = 0;
            for (var i = 0; i < 100; i = i + 1)
            {
                result = result + i;
            }
            result;
        }
        """, 4950)]
    [InlineData("{let hi = \"hellow\" + \" world\" + \" \"; hi;}", "hellow world ")]
    public void Evaluator_Evaluates(string expression, object expectedValue)
    {
        AssertValue(expression, expectedValue);
    }

    static void AssertValue(string expression, object expectedValue)
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
    public void Evaluator_IfStatement_Reports_CannotConvert()
    {
        var text = 
            $$"""
                {
                    var a = 10;
                    if [10]
                    {
                        a = 5;
                    } 
                } 
            """;
        var diagnostics = new[] {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    [Fact]
    public void Evaluator_WhileStatement_Reports_CannotConvert()
    {
        var text = 
            $$"""
                {
                    var a = 10;
                    while [10]
                    {
                        a = 5;
                    } 
                } 
            """;
        var diagnostics = new[] {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_ForStatement_Reports_CannotConvert()
    {
        var text = 
            $$"""
                {
                    var a = 10;
                    for (var i = 0; [10]; i = i + 1)
                    {
                        a = 5;
                    } 
                } 
            """;
        var diagnostics = new[] {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_ForStatement_Reports_Mutation_CannotConvert()
    {
        var text = 
            $$"""
                {
                    var a = 10;
                    for (var i = false; true; i = [1])
                    {
                        a = 5;
                    } 
                } 
            """;
        var diagnostics = new[] {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_OpenBrace_FollowedBy_CloseParenthesise_NoInfiniteLoop()
    {
        var text = 
            $$"""
                {[[)]][]
            """;
        var diagnostics = new[] {
            "error: Unexpected token <CloseParenthesisToken> expected <IdentifierToken>.",
            "error: Unexpected token <CloseParenthesisToken> expected <SemicolonToken>.",
            "error: Unexpected token <EndOfFileToken> expected <CloseBraceToken>.",
        };
        AssertDiagnostics(text, diagnostics);
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
    public void Evaluator_Reports_NoError_For_Inserted_Token()
    {
        var text = "[[]]";
        var diagnostics = new String[] {
            "error: Unexpected token <EndOfFileToken> expected <IdentifierToken>.",
            "error: Unexpected token <EndOfFileToken> expected <SemicolonToken>.",
        };
        AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void Evaluator_ForStatement_Reports_Iterator_Redeclaration()
    {
        var text = 
            $$"""
            for (var i = 1; i < 4; i = i + 1)
            {
                [var i] = 5;
            }
            """;
        var diagnostics = new[] {
            "Variable 'i' is already declared.",
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
            $"Cannot convert '{TypeSymbol.Bool}' to '{TypeSymbol.Int}'.",
        };
        AssertDiagnostics(text, diagnostics);
    }

    static void AssertDiagnostics(string text, string[] diagnosticsText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        diagnostics.Length.Should().Be(diagnosticsText.Length);

        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        for (int i = 0; i < diagnosticsText.Length; i++)
        {
            var expectedMessage = diagnosticsText[i];
            var actualMessage = diagnostics[i].Message;
            actualMessage.Should().Be(expectedMessage, "Diagnostic messages do not match");
            
            
            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].Span;
            actualSpan.Should().Be(expectedSpan, "Diagnostic spans do not match");
        }
    }
}