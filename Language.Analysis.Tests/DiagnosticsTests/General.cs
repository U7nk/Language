using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.DiagnosticsTests;

[SuppressMessage("Usage", "xUnit1015:MemberData must reference an existing member")]
public class General
{
    public General(ITestOutputHelper output)
    {
        Output = output;
    }

    ITestOutputHelper Output { get; set; }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void AssignedExpression_Reports_CannotConvertVariable(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a = 10;
            a = [false]; 
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.BuiltIn.Bool()}' to '{TypeSymbol.BuiltIn.Int()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text,contextType),
            diagnostics);
    }
    
    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void TypeClause_Reports_NoImplicitConversion(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            let a : string = [10 + 15];
            """ ;
        var diagnostics = new[]
        {
            $"No implicit conversion from '{TypeSymbol.BuiltIn.Int()}' to '{TypeSymbol.BuiltIn.String()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void Report_InvalidStatements(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            let a = 5;
            [10 * 5;]
            [a;]
            [a + a;]
            """ ;
        var diagnostics = new[]
        {
            "Only assignment, and call expressions can be used as a statement.",
            "Only assignment, and call expressions can be used as a statement.",
            "Only assignment, and call expressions can be used as a statement.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void IfStatement_Reports_CannotConvert(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a = 10;
            if [10]
            {
                a = 5;
            }
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.BuiltIn.Int()}' to '{TypeSymbol.BuiltIn.Bool()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text,contextType), diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void WhileStatement_Reports_CannotConvert(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a = 10;
            while [10]
            {
                a = 5;
            } 
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.BuiltIn.Int()}' to '{TypeSymbol.BuiltIn.Bool()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType),
            diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void ForStatement_Reports_CannotConvert(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a = 10;
            for (var i = 0; [10]; i = i + 1)
            {
                a = 5;
            }
            """ ;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.BuiltIn.Int()}' to '{TypeSymbol.BuiltIn.Bool()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void ForStatement_Reports_Mutation_NoImplicitConversion(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a = 10;
            for (var i = false; true; i = [1])
            {
                a = 5;
            }
            """;
        var diagnostics = new[]
        {
            $"Cannot convert '{TypeSymbol.BuiltIn.Int()}' to '{TypeSymbol.BuiltIn.Bool()}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Fact]
    
    public void OpenBrace_FollowedBy_CloseParenthesise_NoInfiniteLoop()
    {
        var text =
            $$"""
            namespace Foo
            {
                class Program
                {
                
                    static function main()
                    {
                        {[[)]]
                    }
                }
            }[]
            """ ;
        var diagnostics = new[]
        {
            "Unexpected token <CloseParenthesisToken> expected <IdentifierToken>.",
            "Unexpected token <CloseParenthesisToken> expected <SemicolonToken>.",
            "Unexpected token <EndOfFileToken> expected <CloseBraceToken>.",
        };
        TestTools.AssertDiagnosticsWithMessages(text,diagnostics);
    }


    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void VariableDeclaration_Reports_Redeclaration(TestTools.ContextType contextType)
    {
        var text =
            """  
            var [a] = 10;
            var [a] = 10;
            """;
        var diagnostics = new[]
        {
            "Variable with same name 'a' is already declared.",
            "Variable with same name 'a' is already declared.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType),
            diagnostics);
    }


    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void ForStatement_Reports_Iterator_Redeclaration(TestTools.ContextType contextType)
    {
        var text =
            """
            for (var [i] = 1; i < 4; i = i + 1)
            {
                var [i] = 5;
            }
            """ ;
        var diagnostics = new[]
        {
            "Variable with same name 'i' is already declared.",
            "Variable with same name 'i' is already declared.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType),
            diagnostics);
    }
    
    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void NameExpression_Reports_UndefinedVariable(TestTools.ContextType contextType)
    {
        var text =
            """
            var a = [b];
            """ ;
        var diagnostics = new[]
        {
            "'b' is undefined.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType), 
            diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void AssignedExpression_Reports_CannotAssignVariable(TestTools.ContextType contextType)
    {
        var text =
             """
             let a = 10;
             [a] = 50; 
             """ ;
        var diagnostics = new[]
        {
            "'a' is readonly and cannot be assigned to.",
        };
        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void TypeClause_Reports_UndefinedType(TestTools.ContextType contextType)
    {
        var text =
            """
            var a : [blab] = 10; 
            """ ;
        var diagnostics = new[]
        {
            "Type 'blab' is undefined.",
        };

        TestTools.AssertDiagnosticsWithMessages(
            TestTools.StatementsInContext(text, contextType),
            diagnostics);
    }
    
    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void CannotUseUninitializedVariable(TestTools.ContextType contextType)
    {
        var text =
            """
            var a : int;
            var b = [a];
            b = [a];
            if ([a] == 15){
                b = -[a];
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE,
            DiagnosticBag.REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE,
            DiagnosticBag.REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE,
            DiagnosticBag.REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE,
        };
        
        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), diagnostics, Output);
    }
    
    [Theory]
    [MemberData(
        nameof(TestTools.AllContextTypesForStatements),
        MemberType = typeof(TestTools))]
    public void CannotUseUninitializedVariableDontProduceDiagnosticOnUnreachablePaths(TestTools.ContextType contextType)
    {
        var text =
            """
            var a : int; 
            if (false){
               var b = a;
            }
            var g = [a];
            """;
        var diagnostics = new[]
        {
            DiagnosticBag.REPORT_CANNOT_USE_UNINITIALIZED_VARIABLE_CODE
            
        };
        
        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), diagnostics, Output);
    }
    
    [Fact]
    public void FieldAccessExpressionStatementInvalid()
    {
        var text =
            """
            [StaticField;]
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };

        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, TestTools.ContextType.Method), diagnostics, Output);
    }
    
    [Fact]
    public void FieldAccessExpressionStatementInvalidInInstanceMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program 
                {
                    static StaticField : int;
                    static function main(){
                        
                    }
                    
                    static function StaticMethod(){
                        [StaticField;]
                        [Program.StaticField;]
                    }
                }
            } 
            """ ;
            
        var diagnostics = new[]
        {
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void StaticFieldCannotBeCalledOnInstanceMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program{
                    static StaticField : int;
                    
                    static function main(){    
                    }
                    
                    function StaticMethod(){
                        [[StaticField];]
                    }
                }
            } 
            """ ;
            
        var diagnostics = new[]
        {
            DiagnosticBag.CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE,
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void StaticMethodInsideClassCannotBeCalledOnThis()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    static function main()
                    {
                        
                    }
                    
                    function nonStaticMethod()
                    {
                        this.[staticMethod]();
                    }
                    
                    static function staticMethod() {
                        
                    } 
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void StaticFieldInsideClassCannotBeCalledWithThis()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    static staticField : int;
                    static function main() {  
                        
                    }
                    
                    function method() {
                        var x = this.[staticField];
                    }
                    
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void VariableDeclarationWithoutInitializationRequiresTypeClause()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    static function main() {  
                        var [x];
                    }
                    
                    
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.TYPE_CLAUSE_EXPECTED_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void ThisCannotBeUsedInsideStaticMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    nonStaticField : int;
                    static function main() {  
                        var x = [this].nonStaticField;
                        var y = [this].nonStaticMethod();
                    }
                    
                    function nonStaticMethod() : int {
                        return 0;
                    }
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE,
            DiagnosticBag.THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void MemberAccessReportsAmbiguity()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    static Field : InstanceField;
                    static function main() {   
                        Field.[Foo]();   
                    }
                }
                class Field{
                    static function Foo(){
                        
                    }
                }
                class InstanceField {
                    function Foo(){
                        
                    }
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.AMBIGUOUS_MEMBER_ACCESS_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void MainFunctionShouldBeStatic()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    function [main]() {   
                        var x = 1;   
                    } 
                }
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.MAIN_MUST_HAVE_CORRECT_SIGNATURE_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

    [Fact]
    public void MainMethodShouldBeDeclared()
    {
        var text =
            """
            namespace Project
            {
                class Program
                {
                    function NotMain() {   
                        var x = 1;
                    } 
                }
            }
            """ ;
        var diagnosticsExpected = new[]
        {
            DiagnosticBag.MAIN_METHOD_SHOULD_BE_DECLARED_CODE,
        };
        
        var result = TestTools.Evaluate(text);
        (result is { IsOk: false }).Should().BeTrue();
        
        result.Error.Should().ContainSingle(diagnosticsExpected.Single());
    }
    
    [Fact]
    public void MethodWithGenericTypeParametersCallReportsWhenNoGenericTypeArguments()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    
                    function TestMethod<T>(parameter : T) : T
                        where T : MyClass
                    {
                        return parameter;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var myClass = new MyClass();
                        myClass.[TestMethod](10);
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_GENERIC_ARGUMENTS_NOT_SPECIFIED_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void MethodWithGenericTypeConstraintsCallReportsWhenTypeConstraintDontMatch()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    function TestMethod<T>(parameter : T) : T
                        where T : MyClass
                    {
                        return parameter;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var myClass = new MyClass();
                        myClass.TestMethod<[int]>(10);
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }

    [Fact]
    public void MethodWithGenericTypeConstraintsCallReportsWhenTooMuchTypeArguments()
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
                        myClass.TestMethod[<int, string>](10);
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_WITH_WRONG_GENERIC_ARGUMENTS_COUNT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void MethodWithGenericTypeConstraintsCallReportsWhenNotEnoughTypeArguments()
    {
        var source = """
            namespace Project
            {
                class MyClass
                {
                    function TestMethod<T, TY, TX>(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                class Program
                {
                    static function main()
                    {
                        var myClass = new MyClass();
                        myClass.TestMethod[<int, string>](10);
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_WITH_WRONG_GENERIC_ARGUMENTS_COUNT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    
    [Fact]
    public void InsideGenericClassMethodWithGenericTypeParametersCallReportsWhenNoGenericTypeArguments()
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
                        var myClass = new [MyClass]();
                        myClass.TestMethod(10);
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_GENERIC_ARGUMENTS_NOT_SPECIFIED_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericClassConstraintsShouldReportWhenTheyDoesntSatisfyThemselves()
    {
        var source = """
            namespace Project
            {       
                class MyClass<T> where T : MyClass<[string]>
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
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericClassConstructorCalReportsViolationOfOtherClassGenericConstraints()
    {
        var source = """
            namespace Project
            {            
                class MyClass<T> where T : string
                {
                    function TestMethod(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                
                class SecondClass<T> { }
                class Program
                {
                    static function main()
                    {
                        var f = new SecondClass<MyClass<[int]>>();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericConstraintsOnDeclarationReportsViolationOfOtherClassGenericConstraints()
    {
        var source = """
            namespace Project
            {            
                class MyClass<T> where T : string
                {
                    function TestMethod(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                class SecondClass<T> where T : MyClass<[int]> { }
                
                class Program
                {
                    static function main()
                    {
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void InsideGenericClassMethodWithGenericTypeConstraintsCallReportsWhenTypeConstraintDontMatch()
    {
        var source = """
            namespace Project
            {            
                class MyClass<T> where T : string
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
                        var myClass = new MyClass<[int]>();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void InsideGenericClassMethodWithGenericTypeConstraintsCallReportsWhenNestedTypeConstraintDontMatch()
    {
        var source = """
            namespace Project
            {            
                class MyClass<T> where T : string
                {
                    function TestMethod(parameter : T) : T
                    {
                        return parameter;
                    }
                }
                class SecondClass<T> where T : MyClass<string> { }
                class Program
                {
                    static function main()
                    {
                        var myClass = new MyClass<[int]>();
                        var secondClass = new SecondClass<[MyClass<[int]>]>();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }

    [Fact]
    public void InsideClassMethodWithGenericTypeConstraintsCallReportsWhenTooMuchTypeArguments()
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
                        var myClass = new MyClass[<int, string>]();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_WITH_WRONG_GENERIC_ARGUMENTS_COUNT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void InsideClassMethodWithGenericTypeConstraintsCallReportsWhenNotEnoughTypeArguments()
    {
        var source = """
            namespace Project
            {
                class MyClass<T, TY, TX>
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
                        var myClass = new MyClass[<int, string>]();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            DiagnosticBag.GENERIC_CALL_WITH_WRONG_GENERIC_ARGUMENTS_COUNT_CODE,
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void VariableDeclaredWithWrongGenericTypeReportsWrongGenericArgument()
    {
        var source = """
            namespace Project
            {
                class MyClass<T> where T : string
                {
                   
                }
                class Program
                {
                    static function main()
                    {
                        var myClass : MyClass<[int]>;
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericTypeClauseReportsWrongTypeArguments()
    {
        var source = """
            namespace Project
            {
                class OtherClass<T> where T : string { }
                class MyClass<T> where T : OtherClass<string>
                {
                   function GenericMethod<Y>() where Y : OtherClass<string> { } 
                }
                class Program
                {
                    static function main()
                    {
                        var myClass : MyClass<[OtherClass<[int]>]>; 
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericMethodCallReportsWrongTypeArguments()
    {
        var source = """
            namespace Project
            {
                class OtherClass<T> where T : string { }
                class MyClass<T> where T : OtherClass<string>
                {
                   function GenericMethod<Y>() where Y : OtherClass<string> { } 
                }
                class Program
                {
                    static function main()
                    {
                        var x = new MyClass<OtherClass<string>>();
                        x.GenericMethod<[OtherClass<[int]>]>();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    [Fact]
    public void GenericClassConstructorCallReportsWrongTypeArguments()
    {
        var source = """
            namespace Project
            {
                class OtherClass<T> where T : string { }
                class MyClass<T> where T : OtherClass<string>
                {
                   function GenericMethod<Y>() where Y : OtherClass<string> { } 
                }
                class Program
                {
                    static function main()
                    {
                        var x = new MyClass<[OtherClass<[int]>]>();
                    }
                }
            }
            """;
        
        var diagnostics = new[]
        {
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
            // todo this code is wrong for this type of diagnostic, we need to do create separate code for this case
            DiagnosticBag.GENERIC_METHOD_CALL_WITH_WRONG_TYPE_ARGUMENT_CODE, 
        };
        TestTools.AssertDiagnostics(source, diagnostics, Output);
    }
    
    
    [Fact]
    public void AllGenericTypeUsageIsProperlyParsed()
    {
        var source = """
            namespace MyProgram
            {
                class OtherClass<T> where T : string { }
                class MyClass<T> where T : OtherClass<string>
                {
                   function GenericMethod<Y>() where Y : OtherClass<string> { } 
                }
                class Program
                {
                    static function main()
                    {
                        var myClass : MyClass<OtherClass<string>>;
                        var x = new MyClass<OtherClass<string>>();
                        x.GenericMethod<OtherClass<string>>();
                    }
                }
            }
            """;

        TestTools.Evaluate(source).AssertNoDiagnostics(Output);
    }

}