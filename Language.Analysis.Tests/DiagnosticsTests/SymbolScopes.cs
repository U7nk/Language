using Language.Analysis.CodeAnalysis;

namespace TestProject1.DiagnosticsTests;

public class SymbolScopes
{
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
            DiagnosticBag.CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void VariableCannotHaveSameNameAsParameter()
    {
        const string text = $$"""
            class Program
            {
                function Foo(a : int) {
                    var [a] : int = 0;
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void TwoParametersWithSameName()
    {
        const string text = $$"""
            class Program
            {
                function Foo(a : int, [a] : int) {
                    
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void TwoParametersAndVariableWithSameName()
    {
        const string text = $$"""
            class Program
            {
                function Foo(a : int, [a] : int) {
                    var [a] : int = 5;
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Theory]
    [MemberData(nameof(TestTools.AllContextTypesForStatements), MemberType = typeof(TestTools))]
    public void VariablesCannotHaveSameName(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a : int;
            var [a] : int;
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), diagnostics);
    }
    
    [Theory]
    [MemberData(nameof(TestTools.AllContextTypesForStatements), MemberType = typeof(TestTools))]
    public void VariablesCannotHaveSameNameInNestedScope(TestTools.ContextType contextType)
    {
        var text =
            $$"""
            var a : int; 
            {
                var [a] : int;
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(
            TestTools.StatementsInContext(text, contextType), 
            diagnostics);
    }
    
    
    
    [Fact]
    public void FunctionCannotHaveSameNameAsType()
    {
        var text =
            $$"""
            class TypeName {
                function [TypeName]() {
                    
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void FunctionCannotHaveNameOfField()
    {
        var text =
            $$"""
            class TypeName {
                [field] : int;
                function [field]() {
                    
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void FunctionCannotHaveNameOfFunction()
    {
        var text =
            $$"""
            class TypeName {
                function [FunctionName]() {
                    
                }
                function [FunctionName]() {
                    
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void FieldCannotHaveNameOfField()
    {
        var text =
            $$"""
            class TypeName {
                [field] : int;
                [field] : int;
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE
        };
        
        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void FieldCannotHaveNameOfFunction()
    {
        var text =
            $$"""
            class TypeName {
                function [FunctionName]() {
                    
                }
                [FunctionName] : int;
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE
        };
        
        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void VariableCanShadowField()
    {
        var text =
            $$"""
            class TypeName {
                field : int;
                function FunctionName()
                { 
                    var field : int;
                }
            } 
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void VariableCanShadowFunction()
    {
        var text =
            $$"""
            class TypeName { 
                function FunctionName()
                { 
                    var FunctionName : int;
                }
            } 
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics);
    }
    
    [Fact]
    public void VariableCanShadowClass()
    {
        var text =
            $$"""
            class TypeName { 
                function FunctionName()
                { 
                    var TypeName : int;
                }
            } 
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics);
    }

}