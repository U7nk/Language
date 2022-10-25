using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;

namespace TestProject1.DiagnosticsTests;

public class General
{
    [Fact]
    public void AssignedExpression_Reports_CannotConvertVariable()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }
    [Fact]
    public void TypeClause_Reports_NoImplicitConversion()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Report_InvalidStatements()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void IfStatement_Reports_CannotConvert()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void WhileStatement_Reports_CannotConvert()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void ForStatement_Reports_CannotConvert()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void ForStatement_Reports_Mutation_NoImplicitConversion()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
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
            "error: Unexpected token <CloseParenthesisToken> expected <IdentifierToken>.",
            "error: Unexpected token <CloseParenthesisToken> expected <SemicolonToken>.",
            "error: Unexpected token <EndOfFileToken> expected <CloseBraceToken>.",
        };
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }


    [Fact]
    public void VariableDeclaration_Reports_Redeclaration()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void Reports_NoError_For_Inserted_Token()
    {
        var text = "";
        var diagnostics = Array.Empty<string>();
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void ForStatement_Reports_Iterator_Redeclaration()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void NameExpression_Reports_UndefinedVariable()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void AssignedExpression_Reports_CannotAssignVariable()
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
        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }

    [Fact]
    public void TypeClause_Reports_UndefinedType()
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

        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }
    
    [Fact]
    public void FunctionCannotHaveNameOfType()
    {
        var text =
            $$"""
            class TypeName {
                [TypeName] : int;
                
            } 
            """ ;
        
        var diagnostics = new[]
        {
            "Class member cannot have the same name as the class.",
        };

        TestTools.AssertDiagnosticsWithTimeout(text, diagnostics);
    }
    
}