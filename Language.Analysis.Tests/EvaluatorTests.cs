using System.Collections.Immutable;
using Language;
using Language.Analysis;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace TestProject1;

using FluentAssertions;

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
    [InlineData("\"te\"\"st\";", "te\"st")]
    [InlineData("\"\" == \"\";", true)]
    [InlineData("\" \" != \"\";", true)]
    [InlineData("let a = 10; a * a;", 100)]
    [InlineData("var a = 10; a * a;", 100)]
    [InlineData("var a = 10; a = 5;", 5)]
    [InlineData("var a = 10; if true == true a = 2; a; ", 2)]
    [InlineData("var a = 0; while a < 5 a = a + 1; a;", 5)]
    [InlineData("let hello = \"hello\"; hello;", "hello")]
    [InlineData(
        $$"""
        var x = 10; 
        if true != false
        {
            x = 5;
        }
        x;
      """ , 5)]
    [InlineData(
        $$"""
        var x = 10; 
        if true == false
        {
            x = 5;
        }
        x;
      """ , 10)]
    [InlineData(
        $$"""
         var a = 0; 
         var b = 1;
         while a < 5 
         { 
           a = a + 1;
           b = b + 1; 
         }
         a + b;
      """ , 11)]
    [InlineData(
        $$"""
         var b = 1; 
         var i = 5; 
         for (i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
      """ , 3)]
    [InlineData(
        $$"""
         var b = 1; 
         for (var i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
      """ , 3)]
    [InlineData(
        $$"""
            var result = 0;
            if 1 > 1
                result = 1;
            else 
                result = 2;
            result;
        """ , 2)]
    [InlineData(
        $$"""
        var result = 0;
        result;
        """ , 0)]
    [InlineData(
        $$"""
        var result = 0;
        for (var i = 0; i < 100; i = i + 1)
        {
            result = result + i;
        }
        result;
        """ , 4950)]
    [InlineData("let hi = \"hellow\" + \" world\" + \" \"; hi;", "hellow world ")]
    [InlineData("let boo : string = \"hellow world \"; boo;", "hellow world ")]
    [InlineData(
        $$"""
        function count() : int
        {
            var result = 0;
            for (var i = 0; i < 100; i = i + 1)
            {
                var f = this;
                result = result + i;
            }
            return result;
        }
        count();
        """ , 4950)]
    [InlineData(
        $$"""
        var i = 0; 
        while i < 5 
        {
            i = i + 1;
            if i == 5
                continue;
        }
        i;
        """ , 5)]
    [InlineData(
        $$"""
        var i = 0;
        while false 
        { 
            i = i + 1; 
        } 
        i;
        """ , 0)]
    [InlineData(
        $$"""
        var pp = new Program();
        pp.ten();
        function ten() : int
        {
            return 10;
        }
        """ , 10)]
    public void Evaluator_Evaluates(string expression, object expectedValue)
    {
        AssertValue(expression, expectedValue, isScript: true);
    }

    [Fact]
    public void EvaluatorEvaluatesWithProgramDeclaration()
    {
        AssertValue($$"""
            class Program
            {
                function main()
                {
                    this.ten();
                }
                
                function ten() : int
                {
                    return 10;
                }
            }
            """ ,
            expectedValue: 10, isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesWithFieldsDeclaration()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                
                function main() 
                {
                    this.AssignTenToFieldo(); 
                    var newApp = new Program();
                    newApp.AssignTenToFieldo(); 
                }
                
                function AssignTenToFieldo()
                {
                    this.Fieldo = 10;
                }
            }
            """ ,
            expectedValue: 10, isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesReturnThis()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                
                function main() 
                {
                    this.GetProgram();
                }
                
                function GetProgram() : Program
                {
                    this.Fieldo = 10;
                    return this;
                }
            }
            """ ,
            result =>
            {
                var resultObject = Assert.IsType<Dictionary<string, object>>(result);
                var field = Assert.Single(resultObject);
                field.Key.Should().Be("Fieldo");
                field.Value.Should().Be(10);
            }, isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesMethodsAccessChain()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                
                function main() 
                {
                    this.GetProgram().GetProgram().GetProgram().GetProgram().GetProgram().GetFieldo();
                }
                
                function GetProgram() : Program
                {
                    this.Fieldo = 10;
                    return this;
                }
                
                function GetFieldo() : int
                {
                    return this.Fieldo;
                }
            }
            """ ,
            result => result.Should().Be(10),
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesMethodsFieldsAccessChain()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                ProgramField : Program;
                
                function main() 
                {
                    this.ProgramField = this;
                    this.Fieldo = 10;
                    this.GetProgram()
                            .ProgramField
                            .GetProgram() 
                            .ProgramField
                            .ProgramField
                            .GetProgram()
                            .ProgramField
                            .GetFieldo();
                }
                
                function GetProgram() : Program
                { 
                    return this;
                }
                
                function GetFieldo() : int
                {
                    return this.Fieldo;
                }
            }
            """ ,
            result => result.Should().Be(10),
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesFieldsAssignmentAccessChain()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : Program;
                IntField : int;
                
                function main() 
                { 
                    this.Fieldo = this;
                    this.Fieldo.Fieldo.Fieldo.IntField = 15;
                    var a = this.Fieldo.Fieldo.Fieldo.IntField;
                }
                
                function IntField() : int
                {
                    return this.IntField;
                }
            }
            """ ,
            result => result.Should().Be(15),
            isScript: false);
    }

    static object? EvaluateValue(string expression, bool isScript)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        syntaxTree.Diagnostics.Should().BeEmpty();

        var compilation = isScript ? Compilation.CreateScript(null, syntaxTree) : Compilation.Create(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object?>();
        var evaluation = compilation.Evaluate(variables);

        evaluation.Diagnostics.ToList().Should().BeEmpty();
        return evaluation.Result;
    }

    static void AssertValue(string expression, object expectedValue, bool isScript)
    {
        EvaluateValue(expression, isScript).Should().Be(expectedValue);
    }

    static void AssertValue(string expression, Action<object?> resultAssertion, bool isScript)
    {
        resultAssertion(EvaluateValue(expression, isScript));
    }

    [Fact]
    public void Evaluator_TypeClause_Reports_NoImplicitConversion()
    {
        var text =
            $$"""
            {
                let a : string = [10 + 15];
            } 
            """ ;
        var diagnostics = new[]
        {
            $"No implicit conversion from '{TypeSymbol.Int}' to '{TypeSymbol.String}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Report_InvalidStatements()
    {
        var text =
            $$"""
            {
                let a = 5;
                [10 * 5;]
                [a;]
                [a + a;]
            } 
            """ ;
        var diagnostics = new[]
        {
            $"Only assignment, and call expressions can be used as a statement.",
            $"Only assignment, and call expressions can be used as a statement.",
            $"Only assignment, and call expressions can be used as a statement.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_ForStatement_Reports_Mutation_NoImplicitConversion()
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
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_OpenBrace_FollowedBy_CloseParenthesise_NoInfiniteLoop()
    {
        var text =
            $$"""
                {[[)]][]
            """ ;
        var diagnostics = new[]
        {
            "error: Unexpected token <CloseParenthesisToken> expected <IdentifierToken>.",
            "error: Unexpected token <CloseParenthesisToken> expected <SemicolonToken>.",
            "error: Unexpected token <EndOfFileToken> expected <CloseBraceToken>.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            "Variable 'a' is already declared.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Reports_NoError_For_Inserted_Token()
    {
        var text = "";
        var diagnostics = Array.Empty<string>();
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            "Variable 'i' is already declared.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_NameExpression_Reports_UndefinedVariable()
    {
        var text =
            $$"""
            {
                var a = [b];
            } 
            """ ;
        var diagnostics = new[]
        {
            "'b' is undefined.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
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
             """ ;
        var diagnostics = new[]
        {
            "'a' is readonly and cannot be assigned to.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Evaluator_TypeClause_Reports_UndefinedType()
    {
        var text =
            $$"""
            {
                var a : [blab] = 10;
            } 
            """ ;
        var diagnostics = new[]
        {
            $"Type 'blab' is undefined.",
        };

        AssertDiagnosticsWithTimeout(text, diagnostics);
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
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.Bool}' to '{TypeSymbol.Int}'.",
        };
        AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    static void AssertDiagnostics(string text, string[] diagnosticsText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        diagnostics.Length.Should().Be(diagnosticsText.Length);

        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        foreach (var i in 0..diagnosticsText.Length)
        {
            var expectedMessage = diagnosticsText[i];
            var actualMessage = diagnostics[i].Message;
            actualMessage.Should().Be(expectedMessage, "Diagnostic messages do not match");


            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].TextLocation.Span;
            actualSpan.Should().BeOfType<TextSpan>().And.Be(expectedSpan, "Diagnostic spans do not match");
        }
    }

    static void AssertDiagnosticsWithTimeout(string text, string[] diagnosticsText)
    {
        AssertDiagnostics(text, diagnosticsText);
    }
}