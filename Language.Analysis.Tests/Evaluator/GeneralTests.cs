using System.Collections.Immutable;
using FluentAssertions;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.Evaluator;

public class EvaluatorTests
{
    readonly ITestOutputHelper _testOutputHelper;

    public EvaluatorTests(ITestOutputHelper testOutputHelper)
    {
        this._testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("var p = 1;", 1)]
    [InlineData("var p = (9);", 9)]
    [InlineData("var p = 5 * 2;", 10)]
    [InlineData("var p = 2 + 5 * 2;", 12)]
    [InlineData("var p = (2 + 5) * 2;", 14)]
    [InlineData("var p = +1;", 1)]
    [InlineData("var p = -1;", -1)]
    [InlineData("var p = ~1;", -2)]
    [InlineData("var p = ~6;", -7)]
    [InlineData("var p = ~-7;", 6)]
    [InlineData("var p = -~7;", 8)]
    [InlineData("var p = -1 + 2;", 1)]
    [InlineData("var p = -(1 + 2);", -3)]
    [InlineData("var p = 1 - 2;", -1)]
    [InlineData("var p = 9 / 3;", 3)]
    [InlineData("var p = true;", true)]
    [InlineData("var p = false;", false)]
    [InlineData("var p = !false;", true)]
    [InlineData("var p = !true;", false)]
    [InlineData("var p = !!true;", true)]
    [InlineData("var p = !!false;", false)]
    [InlineData("var p = false || true;", true)]
    [InlineData("var p = false && true;", false)]
    [InlineData("var p = false == true;", false)]
    [InlineData("var p = true == false;", false)]
    [InlineData("var p = true == true;", true)]
    [InlineData("var p = true != true;", false)]
    [InlineData("var p = true != false;", true)]
    [InlineData("var p = false != true;", true)]
    [InlineData("var p = false | false;", false)]
    [InlineData("var p = false | true;", true)]
    [InlineData("var p = true | true;", true)]
    [InlineData("var p = true | false;", true)]
    [InlineData("var p = false & false;", false)]
    [InlineData("var p = false & true;", false)]
    [InlineData("var p = true & true;", true)]
    [InlineData("var p = true & false;", false)]
    [InlineData("var p = false ^ false;", false)]
    [InlineData("var p = false ^ true;", true)]
    [InlineData("var p = true ^ true;", false)]
    [InlineData("var p = 12 == 1;", false)]
    [InlineData("var p = 12 == 12;", true)]
    [InlineData("var p = 12 != 12;", false)]
    [InlineData("var p = 12 != 1;", true)]
    [InlineData("var p = 12 > 1;", true)]
    [InlineData("var p = 12 < 1;", false)]
    [InlineData("var p = 4 >=4;", true)]
    [InlineData("var p = !(4 >= 5);", true)]
    [InlineData("var p = !(4 > 5);", true)]
    [InlineData("var p = !(4 < 5);", false)]
    [InlineData("var p = !(4 <= 5);", false)]
    [InlineData("var p = 4 <= 4;", true)]
    [InlineData("var p = 4 < 4;", false)]
    [InlineData("var p = 3 < 4;", true)]
    [InlineData("var p = 3 > 4;", false)]
    [InlineData("var p = 5 > 4;", true)]
    [InlineData("var p = 1 | 2;", 3)]
    [InlineData("var p = 1 & 2;", 0)]
    [InlineData("var p = 1 & 3;", 1)]
    [InlineData("var p = 1 ^ 2;", 3)]
    [InlineData("var p = 1 ^ 3;", 2)]
    [InlineData("var p = \"te\"\"st\";", "te\"st")]
    [InlineData("var p = \"\" == \"\";", true)]
    [InlineData("var p = \" \" != \"\";", true)]
    [InlineData("let a = 10; var p = a * a;", 100)]
    [InlineData("var a = 10; var p = a * a;", 100)]
    [InlineData("var a = 10; a = 5;", 5)]
    [InlineData("var a = 10; if true == true a = 2; var p = a; ", 2)]
    [InlineData("var a = 0; while a < 5 a = a + 1; var p = a;", 5)]
    [InlineData("let hello = \"hello\"; var p = hello;", "hello")]
    [InlineData(
        $$"""
        var x = 10; 
        if true != false
        {
            x = 5;
        }
        var p = x;
      """, 5)]
    [InlineData(
        $$"""
        var x = 10; 
        if true == false
        {
            x = 5;
        }
        var p = x;
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
         var p = a + b;
      """, 11)]
    [InlineData(
        $$"""
         var b = 1; 
         var i = 5; 
         for (i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         var p = b;
      """, 3)]
    [InlineData(
        $$"""
         var b = 1; 
         for (var i = 1; i < 4; i = i + 1)
         {
            b = i;
         } 
         var p = b;
      """, 3)]
    [InlineData(
        $$"""
            var result = 0;
            if 1 > 1
                result = 1;
            else 
                result = 2;
            var p = result;
        """, 2)]
    [InlineData(
        $$"""
        var result = 0;
        var p = result;
        """, 0)]
    [InlineData(
        $$"""
        var result = 0;
        for (var i = 0; i < 100; i = i + 1)
        {
            result = result + i;
        }
        var p = result;
        """, 4950)]
    [InlineData("let hi = \"hellow\" + \" world\" + \" \"; var p = hi;", "hellow world ")]
    [InlineData("let boo : string = \"hellow world \"; var p = boo;", "hellow world ")]
    //[InlineData(
    //    $$"""
    //    function count() : int
    //    {
    //        var result = 0;
    //        for (var i = 0; i < 100; i = i + 1)
    //        {
    //            var f = "hello";
    //            result = result + i;
    //        }
    //        return result;
    //    }
    //    count();
    //    """, 4950)]
    [InlineData(
        $$"""
        var i = 0; 
        while i < 5 
        {
            i = i + 1;
            if i == 5
                continue;
        }
        var p = i;
        """, 5)]
    [InlineData(
        $$"""
        var i = 0;
        while false 
        { 
            i = i + 1; 
        } 
        var p = i;
        """, 0)]
    //[InlineData(
    //    $$"""
    //    ten();
    //    function ten() : int
    //    {
    //        return 10;
    //    }
    //    """, 10)]
    public void Evaluator_Evaluates(string expression, object expectedValue)
    {
        var fullCode =  $$"""
                        namespace Foo
                        {
                            class Program
                            {
                                static function main()
                                {
                                    {{expression}}
                                }
                            }
                        }
                        """;
        AssertValue(fullCode, expectedValue);
    }

    [Fact]
    public void EvaluatorEvaluatesWithProgramDeclaration()
    {
        AssertValue("""
            namespace Project
            {
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
            }
            """,
                    expectedValue: 10);
    }

    [Fact]
    public void EvaluatorEvaluatesWithFieldsDeclaration()
    {
        AssertValue("""
            namespace Project
            {
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
            }
            """,
                    expectedValue: 10);
    }

    [Fact]
    public void EvaluatorEvaluatesReturnThis()
    {
        AssertValue("""
                    namespace Project
                    {
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
                    }
                    """, 
                    result =>
                    {
                        var resultObject = Assert.IsType<ObjectInstance>(result);
                        var field = Assert.Single(resultObject.Fields);
                        field.Key.Should().Be("Fieldo");
                        field.Value.NullGuard().LiteralValue.Should().Be(10);
                    });
    }

    [Fact]
    public void EvaluatorEvaluatesMethodsAccessChain()
    {
        AssertValue("""
            namespace Project
            {
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
            }
            """,
                    result =>
                    {
                        Assert.NotNull(result);
                        result.Type.Should().Be(TypeSymbol.BuiltIn.Int());
                        result.LiteralValue.Should().Be(10);
                    });
    }

    [Fact]
    public void EvaluatorEvaluatesMethodsFieldsAccessChain()
    {
        AssertValue("""
            namespace Project
            {
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
            }
            """,
                    result =>
                    {
                        result.Should().NotBeNull();
                        result.NullGuard().Type.Should().Be(TypeSymbol.BuiltIn.Int());
                        result.LiteralValue.Should().Be(10);
                    });
    }

    [Fact]
    public void EvaluatorEvaluatesFieldsAssignmentAccessChain()
    {
        AssertValue("""
            namespace Project
            {
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
            }
            """,
                    result =>
                    {
                        result.Should().NotBeNull();
                        result.NullGuard().Type.Should().Be(TypeSymbol.BuiltIn.Int());
                        result.NullGuard().LiteralValue.Should().Be(15);
                    });
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
            _ => { });
    }

    
    [Fact]
    public void EvaluatorEvaluatesAssignmentToUninitializedVariable()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
                class Program
                {                
                    static function main()
                    {  
                        var variable : int;
                        variable = {{val}};
                    }
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NullGuard().LiteralValue.Should().Be(val); });
    }
    
    [Fact]
    public void EvaluatorEvaluatesMethodCallInsideClassWithoutThis()
    {
        var source = """
            namespace Project
            {
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
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.Int());
                result.LiteralValue.Should().Be(10);
            });
    }

    [Fact]
    public void EvaluatorEvaluatesFieldAccessInsideClassWithoutThis()
    {
        var source = """
            namespace Project
            {
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
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.Int());
                result.LiteralValue.Should().Be(10);
            });
    }

    [Fact]
    public void EvaluatorEvaluatesStaticMethodCall()
    {
        var source = """
            namespace Project
            {
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
            }
            """;
        AssertValue(
            source,
            result => { (result is { LiteralValue: 10 }).EnsureTrue("\n" + result); });
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAssignment()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
                class Program
                {
                    static Field : int;
                    
                    static function main()
                    {  
                        Program.Field = {{val}};
                    }
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NullGuard().LiteralValue.Should().Be(val); })
            ;
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAccess()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
                class Program
                {
                    static Field : int;
                    
                    static function main()
                    {  
                        Program.Field = {{val}};
                        var x = Program.Field;
                    }
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NullGuard().LiteralValue.Should().Be(val); });
    }

    [Fact]
    public void EvaluatorEvaluatesAssignmentToStaticFieldInsideCurrentClassWithoutThis()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
                class Program
                {
                    static Field : int;
                    
                    static function main()
                    {  
                        Field = {{val}};
                    }
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NullGuard().LiteralValue.Should().Be(val); });
    }

    [Fact]
    public void EvaluatorEvaluatesAccessToStaticFieldInsideCurrentClassWithoutThis()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
                class Program
                {
                    static Field : int;
                    
                    static function main()
                    {  
                        Field = {{val}};
                        var x = Field;
                    }
                }
            }
            """;
        AssertValue(
            source,
            result => { result.NullGuard().LiteralValue.Should().Be(val); });
    }

    [Fact]
    public void EvaluatorEvaluatesStaticFieldAccessWithNameAmbiguityResolve()
    {
        var val = Random.Shared.NextInt64(0, 115);
        var source = $$"""
            namespace Project
            {
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
            }
            """;
        var res = TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
        res.IsOk.Should().Be(true);
        res.Ok.NullGuard().LiteralValue.Should().Be(val);
    }
    
    [Fact]
    public void EvaluatorEvaluatesStaticMethodCallInsideClassWithoutThis()
    {
        var source = """
            namespace Project
            {
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
            }
            """;
        AssertValue(
            source,
            result => { (result is { LiteralValue: 10 }).EnsureTrue("\n"+result); });
    }

    [Fact]
    public void EvaluatorEvaluatesFieldAssignmentWithoutThis()
    {
        var source = """
            namespace Project
            {
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
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.Int());
                result.LiteralValue.Should().Be(10);
            });
    }
    
    [Fact]
    public void EvaluatorEvaluatesMethodCallOnField()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    Field : MyClass;
                    function TestMethod() : int
                    {
                        return 10;
                    }
                    
                    function TestMethodTwo()
                    { 
                        this.Field.TestMethod();
                    }
                }
                class Program
                {
                    Field : int;
                    static function main()
                    {
                        var program = new MyClass();
                        program.Field = new MyClass();
                        program.TestMethodTwo();
                    }
                }
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.Int());
                result.LiteralValue.Should().Be(10);
            },
            
            Option.Some(_testOutputHelper));
    }
    
    [Fact]
    public void EvaluatorEvaluatesMethodWithGenericParameterCall()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    
                    function TestMethod<T>(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var myClass = new MyClass();
                        myClass.TestMethod<int>(10);
                        myClass.TestMethod<string>("foo");
                    }
                }
            }
            """;
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.String());
                result.LiteralValue.Should().Be("foo");
            },
            
            Option.Some(_testOutputHelper));
    }
    
    [Fact]
    public void EvaluatorEvaluatesMethodWithClassGenericParameterCall()
    {
        var source = """
            namespace Project
            {
                class MyClass<T>
                {
                
                    function TestMethod(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var myClassString = new MyClass<string>();
                        var myClassInt = new MyClass<int>();
                        myClassInt.TestMethod(10);
                        myClassString.TestMethod("foo");
                    }
                }
            }
            """;
        
        AssertValue(
            source,
            result =>
            {
                Assert.NotNull(result);
                result.Type.Should().Be(TypeSymbol.BuiltIn.String());
                result.LiteralValue.Should().Be("foo");
            },
            
            Option.Some(_testOutputHelper));
    }
    
    [Fact]
    public void MethodWithGenericTypeConstraintsCallReportsWhenTypeConstraintDontMatch()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    function TestMethod<T>(parameter : T) : int
                        where T : MyClass
                    {
                        return parameter.TestMethodTwo();
                    }
                
                    function TestMethodTwo() : int
                    {
                        return 10;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var result = new MyClass().TestMethod<MyClass>(new MyClass());
                    }
                }
            }
            """;
        
        (TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper).Ok is { LiteralValue: 10, Type.Name: "int" })
            .Should().BeTrue();
    }
    
    [Fact]
    public void NamespaceIsCorrectlyParsed()
    {
        var source = """
            namespace MyProgram
            {
                class Program
                {
                    static function main()
                    {
                        
                    }
                }
            }
            """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }
    
    [Fact]
    public void MultiPartNamespaceIsCorrectlyParsed()
    {
        var source = """
            namespace MyNamespace.MyProgram
            {
                class Program
                {
                    static function main()
                    {
                        
                    }
                }
            }
            """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }
    
    [Fact]
    public void LongNamespaceIsCorrectlyParsed()
    {
        var source = """
                     namespace MyNamespace.MyProgram.My.Everything
                     {
                         class Program
                         {
                             static function main()
                             {
                                 
                             }
                         }
                     }
                     
                     namespace Other
                     {
                        class Program
                        {
                            static function main()
                            {
                         
                            }
                        }
                     }
                     """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }

    [Fact]
    public void FullNamespaceWithNewKeyword()
    {
        var source = """
                     namespace MyNamespace.MyProgram.My.Everything
                     {
                         class Program
                         {
                             static function main()
                             {
                                var x = new MyNamespace.MyProgram.My.Everything.Program();
                             }
                         }
                     }
                     """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }
    
    [Fact]
    public void MultipleNamespacesEvaluateNoDiagnostic()
    {
        var source = """
                     namespace Foo
                     {
                        
                     }
                     namespace MyNamespace.MyProgram
                     {
                         class Program
                         {
                             static function main()
                             {
                             }
                         }
                     }
                     """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }
    
    [Fact]
    public void Full_Class_Name_With_Namespace_Can_Be_Used_In_Generic_Constraints_Of_Type_In_Other_Namespace()
    {
        var source = """
                     namespace Foo
                     {
                        class ClassInOtherNamespace
                        {
                        }
                     }
                     namespace MyNamespace.MyProgram
                     {
                         class GenericConstrainedClass<T> where T : Foo.ClassInOtherNamespace
                         {
                         }
                         class Program
                         {
                             static function main()
                             {
                             }
                         }
                     }
                     """;

        TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
    }
    
    [Fact]
    public void MethodCallInvokedByNameWithNamespaceEvaluateCorrect()
    {
        var source = """
                     namespace Foo
                     {
                        class Class
                        {
                            static function method() : int
                            {
                                return 13;
                            }
                        }
                     }
                     namespace MyNamespace.MyProgram
                     {
                         class Program
                         {
                             static function main()
                             {
                                Foo.Class.method();
                             }
                         }
                     }
                     """;

        var res = TestTools.Evaluate(source).AssertNoDiagnostics(_testOutputHelper);
        res.Ok.Type.Name.Should().Be("int");
        res.Ok.LiteralValue.Should().Be(13);
    }
    
    static ObjectInstance? EvaluateValue(string expression, Option<ITestOutputHelper> output = default)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        if (output.IsSome)
        {
            if (syntaxTree.Diagnostics is { Count: >0 } diagnostics)
            {
                new Result<ObjectInstance?, ImmutableArray<Diagnostic>>(diagnostics.ToImmutableArray())
                    .AssertNoDiagnostics(output.Unwrap());
            }
        }
        else
        {
            syntaxTree.Diagnostics.Should().BeEmpty();
        }

        var compilation =  Compilation.Create(syntaxTree);
        var variables = new Dictionary<VariableSymbol, ObjectInstance?>();
        var evaluation = compilation.Evaluate(variables);

        if (output.IsSome)
        {
            Result<ObjectInstance?, ImmutableArray<Diagnostic>> evaluationCastedToResult = evaluation;
            evaluationCastedToResult.AssertNoDiagnostics(output.Unwrap());
        }
        else
        {
            evaluation.Diagnostics.AsEnumerable().Should().BeEmpty();   
        }
        return evaluation.Result;
    }

    static void AssertValue(string expression, object expectedValue)
    {
        EvaluateValue(expression).NullGuard().LiteralValue.Should().Be(expectedValue);
    }

    static void AssertValue(string expression, Action<ObjectInstance?> resultAssertion, Option<ITestOutputHelper> output = default)
    {
        resultAssertion(EvaluateValue(expression, output));
    }
}