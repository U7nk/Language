using System.Collections.Immutable;
using Language;
using Language.Analysis;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Interpretation;
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
      """, 5)]
    [InlineData(
        $$"""
        var x = 10; 
        if true == false
        {
            x = 5;
        }
        x;
      """, 10)]
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
      """, 11)]
    [InlineData(
        $$"""
         var b = 1; 
         var i = 5; 
         for (i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
      """, 3)]
    [InlineData(
        $$"""
         var b = 1; 
         for (var i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         b;
      """, 3)]
    [InlineData(
        $$"""
            var result = 0;
            if 1 > 1
                result = 1;
            else 
                result = 2;
            result;
        """, 2)]
    [InlineData(
        $$"""
        var result = 0;
        result;
        """, 0)]
    [InlineData(
        $$"""
        var result = 0;
        for (var i = 0; i < 100; i = i + 1)
        {
            result = result + i;
        }
        result;
        """, 4950)]
    [InlineData("let hi = \"hellow\" + \" world\" + \" \"; hi;", "hellow world ")]
    [InlineData("let boo : string = \"hellow world \"; boo;", "hellow world ")]
    [InlineData(
        $$"""
        function count() : int
        {
            var result = 0;
            for (var i = 0; i < 100; i = i + 1)
            {
                var f = "hello";
                result = result + i;
            }
            return result;
        }
        count();
        """, 4950)]
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
        """, 5)]
    [InlineData(
        $$"""
        var i = 0;
        while false 
        { 
            i = i + 1; 
        } 
        i;
        """, 0)]
    [InlineData(
        $$"""
        ten();
        function ten() : int
        {
            return 10;
        }
        """, 10)]
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
                static function main()
                {
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod()
                {
                    this.ten();
                }
                
                function ten() : int
                {
                    return 10;
                }
            }
            """,
                    expectedValue: 10, isScript: false);
    }

    [Fact]
    public void EvaluatorTopLevelStatementMethodsCanBeCalledWithoutMembers()
    {
        AssertValue($$"""
            function ten() : int
            {
                return 10;
            }
            ten();
            """,
                    expectedValue: 10,
                    isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesWithFieldsDeclaration()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                
                static function main() 
                {
                     var p = new Program();
                     p.TestMethod();
                }
                
                function TestMethod() {
                    this.AssignTenToFieldo(); 
                    var newApp = new Program();
                    newApp.AssignTenToFieldo();
                }
                
                function AssignTenToFieldo()
                {
                    this.Fieldo = 10;
                }
            }
            """,
                    expectedValue: 10, isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesReturnThis()
    {
        AssertValue($$"""
                    class Program
                    {
                        Fieldo : int;
                        
                        static function main() 
                        {
                            var p = new Program();
                            p.TestMethod();
                        }
                        
                        function TestMethod() {
                            this.GetProgram();
                        }
                        
                        function GetProgram() : Program
                        {
                            this.Fieldo = 10;
                            return this;
                        }
                    }
                    """, 
                    result =>
                    {
                        var resultObject = Assert.IsType<ObjectInstance>(result);
                        var field = Assert.Single(resultObject.Fields);
                        field.Key.Should().Be("Fieldo");
                        field.Value.NG().LiteralValue.Should().Be(10);
                    }, isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesMethodsAccessChain()
    {
        AssertValue($$"""
            class Program
            {
                Fieldo : int;
                
                static function main() 
                {
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod(){
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
            """,
                    result =>
                    {
                        Assert.NotNull(result);
                        result.Type.Should().Be(TypeSymbol.Int);
                        result.LiteralValue.Should().Be(10);
                    },
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
                
                static function main() 
                {
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod(){
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
            """,
                    result =>
                    {
                        result.Should().NotBeNull();
                        result.NG().Type.Should().Be(TypeSymbol.Int);
                        result.LiteralValue.Should().Be(10);
                    },
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
                
                static function main() 
                { 
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod() {
                    this.Fieldo = this;
                    this.Fieldo.Fieldo.Fieldo.IntField = 15;
                    var a = this.Fieldo.Fieldo.Fieldo.IntField;
                }
                
                function GetIntField() : int
                {
                    return this.IntField;
                }
            }
            """,
                    result =>
                    {
                        result.Should().NotBeNull();
                        result.NG().Type.Should().Be(TypeSymbol.Int);
                        result.NG().LiteralValue.Should().Be(15);
                    },
                    isScript: false);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void EvaluatorEvaluatesVariableDeclaration(TestTools.ContextType contextType)
    {
        var source = $$"""
            var a : int;
            """;
        AssertValue(
            TestTools.StatementsInContext(source, contextType),
            _ => { },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesMethodCallInsideClassWithoutThis()
    {
        var source = $$"""
            class Program
            {
                static function main() 
                {
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod() {
                    ten();
                }
                
                function ten() : int
                {
                    return 10;
                }
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.Int);
                result.LiteralValue.Should().Be(10);
            },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesFieldAccessInsideClassWithoutThis()
    {
        var source = $$"""
            class Program
            {
                Field : int;
                static function main()
                {
                    var p = new Program();
                    p.TestMethod();
                }
                
                function TestMethod() {
                    Field = 10; 
                    this.ten(Field);
                }
                
                function ten(input : int) : int
                {
                    return 10;
                }
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.Int);
                result.LiteralValue.Should().Be(10);
            },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesStaticMethodCall()
    {
        var source = """
            class Program
            {
                Field : int;
                
                static function main()
                { 
                    var program = new Program();
                    program.Field = 10;
                    Program.ten(program.Field);
                }
                
                static function ten(input : int) : int
                {
                    return 10;
                }
            }
            """;
        AssertValue(
            source,
            result => { (result is { LiteralValue: 10 }).ThrowIfFalse("\n" + result); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAssignment()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            class Program
            {
                static Field : int;
                
                static function main()
                {  
                    Program.Field = {{val}};
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NG().LiteralValue.Should().Be(val); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAccess()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            class Program
            {
                static Field : int;
                
                static function main()
                {  
                    Program.Field = {{val}};
                    var x = Program.Field;
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NG().LiteralValue.Should().Be(val); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesAssignmentToStaticFieldInsideCurrentClassWithoutThis()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            class Program
            {
                static Field : int;
                
                static function main()
                {  
                    Field = {{val}};
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NG().LiteralValue.Should().Be(val); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesAccessToStaticFieldInsideCurrentClassWithoutThis()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            class Program
            {
                static Field : int;
                
                static function main()
                {  
                    Field = {{val}};
                    var x = Field;
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NG().LiteralValue.Should().Be(val); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAccessWithNameAmbiguityResolve()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            class Program
            {
                static Field : InstanceClass;
                
                static function main()
                {
                    Field.InstanceClass = new InstanceClass();
                    Field.InstanceClass.Foo = {{val}};
                    var x = Field.InstanceClass.Foo;
                }
            }
            class Field {
                static InstanceClass : InstanceClass;
            }
            
            class InstanceClass {
                Foo : int;
            }
            """;
        AssertValue(
            source,
            result => { result.NG().LiteralValue.Should().Be(val); },
            isScript: false);
    }
    
    [Fact]
    public void EvaluatorEvaluatesStaticMethodCallInsideClassWithoutThis()
    {
        var source = $$"""
            class Program
            {
                static Field : int;
                static function main()
                {
                    Field = 10; 
                    ten(Field);
                }
                
                static function ten(input : int) : int
                {
                    return 10;
                }
            }
            """;
        AssertValue(
            source,
            result => { (result is { LiteralValue: 10 }).ThrowIfFalse("\n"+result); },
            isScript: false);
    }

    [Fact]
    public void EvaluatorEvaluatesFieldAssignmentWithoutThis()
    {
        var source = """
            class Program
            {
                Field : int;
                static function main()
                {
                    var program = new Program();
                    program.TestMethod();
                }
                
                function TestMethod() {
                    Field = 10;
                }
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.Int);
                result.LiteralValue.Should().Be(10);
            },
            isScript: false);
    }


    static ObjectInstance? EvaluateValue(string expression, bool isScript)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        syntaxTree.Diagnostics.Should().BeEmpty();

        var compilation = isScript ? Compilation.CreateScript(null, syntaxTree) : Compilation.Create(syntaxTree);
        var variables = new Dictionary<VariableSymbol, ObjectInstance?>();
        var evaluation = compilation.Evaluate(variables);

        evaluation.Diagnostics.ToList().Should().BeEmpty();
        return evaluation.Result;
    }

    static void AssertValue(string expression, object expectedValue, bool isScript)
    {
        EvaluateValue(expression, isScript).NG().LiteralValue.Should().Be(expectedValue);
    }

    static void AssertValue(string expression, Action<ObjectInstance?> resultAssertion, bool isScript)
    {
        resultAssertion(EvaluateValue(expression, isScript));
    }
}