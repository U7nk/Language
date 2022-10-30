using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace TestProject1.DiagnosticsTests;

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
            $"Cannot convert '{TypeSymbol.Bool}' to '{TypeSymbol.Int}'.",
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
            $"No implicit conversion from '{TypeSymbol.Int}' to '{TypeSymbol.String}'.",
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
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
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
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
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
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
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
            $"Cannot convert '{TypeSymbol.Int}' to '{TypeSymbol.Bool}'.",
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
        var text = "";
        var diagnostics = Array.Empty<string>();
        TestTools.AssertDiagnosticsWithMessages(text, diagnostics);
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
            [this.Field;]
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };

        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, TestTools.ContextType.Method), diagnostics, Output);
    }
    
    [Fact]
    public void StaticMethodInsideClassCannotBeCalledWithThis()
    {
        var text =
            $$"""
            class Program
            {
                static function Main()
                {
                    this.[staticMethod]();
                }
                static function staticMethod() {
                
                } 
            }
            """ ;
        var diagnostics = new[]
        {
            DiagnosticBag.INVALID_EXPRESSION_STATEMENT_CODE,
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

}