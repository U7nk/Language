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
            $"Cannot convert '{BuiltInTypeSymbols.Bool}' to '{BuiltInTypeSymbols.Int}'.",
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
            $"No implicit conversion from '{BuiltInTypeSymbols.Int}' to '{BuiltInTypeSymbols.String}'.",
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
            $"Cannot convert '{BuiltInTypeSymbols.Int}' to '{BuiltInTypeSymbols.Bool}'.",
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
            $"Cannot convert '{BuiltInTypeSymbols.Int}' to '{BuiltInTypeSymbols.Bool}'.",
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
            $"Cannot convert '{BuiltInTypeSymbols.Int}' to '{BuiltInTypeSymbols.Bool}'.",
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
            $"Cannot convert '{BuiltInTypeSymbols.Int}' to '{BuiltInTypeSymbols.Bool}'.",
        };
        TestTools.AssertDiagnosticsWithMessages(TestTools.StatementsInContext(text, contextType), diagnostics);
    }

    [Fact]
    
    public void OpenBrace_FollowedBy_CloseParenthesise_NoInfiniteLoop()
    {
        var text =
            $$"""
                {[[)]][]
            """ ;
        var diagnostics = new[]
        {
            "Unexpected token <CloseParenthesisToken> expected <IdentifierToken>.",
            "Unexpected token <CloseParenthesisToken> expected <SemicolonToken>.",
            "Unexpected token <EndOfFileToken> expected <CloseBraceToken>.",
        };
        TestTools.AssertDiagnosticsWithMessages(text, diagnostics);
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

    [Fact]
    public void Reports_NoError_For_Inserted_Token()
    {
        var text = "[]";
        var diagnostics = new[]
        {
            DiagnosticBag.MAIN_METHOD_SHOULD_BE_DECLARED_CODE
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
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
        
        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), false, diagnostics, Output);
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
        
        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), false, diagnostics, Output);
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

        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, TestTools.ContextType.Method), false, diagnostics, Output);
    }
    
    [Fact]
    public void FieldAccessExpressionStatementInvalidInInstanceMethod()
    {
        var text =
            """
            class Program{
                static StaticField : int;
                InstanceField : int;
                
                static function main(){
                    
                }
                
                function StaticMethod(){
                    [StaticField;]
                    [Program.StaticField;]
                }
            } 
            """ ;
            
        var diagnostics = new[]
        {
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };

        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void StaticMethodInsideClassCannotBeCalledOnThis()
    {
        var text =
            $$"""
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
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void StaticFieldInsideClassCannotBeCalledWithThis()
    {
        var text =
            """
            class Program
            {
                static staticField : int;
                static function main() {  
                    
                }
                
                function method() {
                    var x = this.[staticField];
                }
                
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.CANNOT_ACCESS_STATIC_ON_NON_STATIC_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void VariableDeclarationWithoutInitializationRequiresTypeClause()
    {
        var text =
            """
            class Program
            {
                static function main() {  
                    var [x];
                }
                
                
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.TYPE_CLAUSE_EXPECTED_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void ThisCannotBeUsedInsideStaticMethod()
    {
        var text =
            """
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
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE,
            DiagnosticBag.THIS_EXPRESSION_NOT_ALLOWED_IN_STATIC_CONTEXT_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void MemberAccessReportsAmbiguity()
    {
        var text =
            """
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
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.AMBIGUOUS_MEMBER_ACCESS_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }
    
    [Fact]
    public void MainFunctionShouldBeStatic()
    {
        var text =
            """
            class Program
            {
                function [main]() {   
                    var x = 1;   
                } 
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.MAIN_MUST_HAVE_CORRECT_SIGNATURE_CODE,
        };
        TestTools.AssertDiagnostics(text, false, diagnostics, Output);
    }

    [Fact]
    public void MainMethodShouldBeDeclared()
    {
        var text =
            """
            class Program
            {
                function NotMain() {   
                    var x = 1;
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
    public void NoMainMethodAllowedInScriptMode()
    {
        var text =
            """
            class Program
            {
                static function [main]() {   
                    var x = 1;   
                } 
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.NO_MAIN_METHOD_ALLOWED_IN_SCRIPT_MODE_CODE,
        };
        TestTools.AssertDiagnostics(text, isScript: true, diagnostics, Output);
    }
}