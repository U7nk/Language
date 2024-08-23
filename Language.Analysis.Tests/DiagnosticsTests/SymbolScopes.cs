using System.Collections.Immutable;
using FluentAssertions;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.DiagnosticsTests;

public class SymbolScopes
{
    public SymbolScopes(ITestOutputHelper output)
    {
        Output = output;
    }

    ITestOutputHelper Output { get; set; }

    [Fact]
    public void FieldCannotHaveNameOfType()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    [Program] : int;
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void VariableCannotHaveSameNameAsParameter()
    {
        const string text = """
            namespace Project
            {
                class Program
                {
                    static function main(){
                    }
                    function Foo([a] : int) {
                        var [a] : int = 0;
                    }
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

    [Fact]
    public void TwoParametersWithSameName()
    {
        const string text = """
            namespace Project
            {
                class Program
                {
                    static function main(){
                    }
                    function Foo([a] : int, [a] : int) {
                        
                    }
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void TwoParametersAndVariableWithSameName()
    {
        var text = """
            namespace Project
            {
                class Program
                {
                    static function main(){
                    }
                    function Foo([[a]] : int, [[a]] : int) {
                        var [a] : int = 5;
                    }
                }
            }
            """.Replace(Environment.NewLine, "\n\r");
        
        TestTools.AssertDiagnostics(text, new []
        {
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
            DiagnosticBag.PARAMETER_ALREADY_DECLARED_CODE,
        }, Output);
    }
    
    [Theory]
    [MemberData(nameof(TestTools.AllContextTypesForStatements), MemberType = typeof(TestTools))]
    public void VariablesCannotHaveSameName(TestTools.ContextType contextType)
    {
        var text =
            """
            var [a] : int;
            var [a] : int;
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(TestTools.StatementsInContext(text, contextType), diagnostics, Output);
    }
    
    [Theory]
    [MemberData(nameof(TestTools.AllContextTypesForStatements), MemberType = typeof(TestTools))]
    public void VariablesCannotHaveSameNameInNestedScope(TestTools.ContextType contextType)
    {
        var text =
            """
            var [a] : int; 
            {
                var [a] : int;
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
            DiagnosticBag.VARIABLE_ALREADY_DECLARED_CODE,
        };

        TestTools.AssertDiagnostics(
            TestTools.StatementsInContext(text, contextType), 
            diagnostics, Output);
    }

    [Fact]
    public void MethodCannotHaveSameNameAsType()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    
                    function [Program]() {
                        
                    }
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_CANNOT_HAVE_NAME_OF_CLASS_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

    [Fact]
    public void MethodCannotHaveNameOfField()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    [field] : int;
                    function [field]() {
                        
                    }
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void MethodCannotHaveNameOfMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    function [FunctionName]() {
                        
                    }
                    function [FunctionName]() {
                        
                    }
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.METHOD_ALREADY_DECLARED_CODE,
            DiagnosticBag.METHOD_ALREADY_DECLARED_CODE
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

    [Fact]
    public void MethodCannotHaveNameOfVirtualMethodFromBaseClass()
    {
        var text =
            """
            namespace Project
            {
                class Base {
                    function virtual FunctionName() { }
                }
                class Inherited : Base {
                    function [FunctionName]() { }
                } 
                class Program {
                    static function main(){
                    }
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.METHOD_ALREADY_DECLARED_IN_BASE_CLASS_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void OverrideMethodDoesntReturnDiagnosticWhenHaveNameOfVirtualMethodFromBaseClass()
    {
        var text =
            """
            namespace Project
            {
                class Base {
                    function virtual FunctionName() { }
                }
                class Inherited : Base {
                    function override [FunctionName]() { }
                    function virtual [[FunctionName]]() { }
                } 
                class Program {
                    static function main(){
                    }
                }
            } 
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.METHOD_ALREADY_DECLARED_CODE,
            DiagnosticBag.METHOD_ALREADY_DECLARED_CODE,
            DiagnosticBag.METHOD_ALREADY_DECLARED_IN_BASE_CLASS_CODE,
        };

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void OverrideMethodCanHaveNameOfVirtualMethodFromBaseClass()
    {
        var text =
            """
            namespace Project
            {
                class Base {
                    function virtual FunctionName() { }
                }
                class Inherited : Base {
                    function override FunctionName() { }
                } 
                class Program {
                    static function main(){
                    }
                }
            } 
            """;

        TestTools.Evaluate(text).AssertNoDiagnostics(Output);
    }
    
    [Fact]
    public void MethodCannotHaveSameNameOfMethodInBaseClass()
    {
        var text =
            """
            namespace Project
            {
                class BaseClass {
                    function FunctionName() { }
                }
                
                class DerivedClass : BaseClass {
                    function [FunctionName]() { }
                }
                
                class Program {
                    static function main() { }
                }
            } 
            """;

        var diagnostics = new[]
        {
            DiagnosticBag.METHOD_ALREADY_DECLARED_IN_BASE_CLASS_CODE
        };
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void FieldCannotHaveNameOfField()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    
                    [field] : int;
                    [field] : int;
                }
            } 
            """ ;

        var diagnostics = new[]
        {
            DiagnosticBag.FIELD_ALREADY_DECLARED_CODE,
            DiagnosticBag.FIELD_ALREADY_DECLARED_CODE,
        };
        
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void FieldCannotHaveNameOfFieldAndMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    
                    function [field]() { }
                    
                    [[field]] : int;
                    [[field]] : int;
                }
            }
            """ ;

        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
            
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
            DiagnosticBag.FIELD_ALREADY_DECLARED_CODE,
            
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
            DiagnosticBag.FIELD_ALREADY_DECLARED_CODE,
        };
        
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void FieldCannotHaveNameOfMethod()
    {
        
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    
                    function [FunctionName]() {
                        
                    }
                    [FunctionName] : int;
                }
            }
            """ ;
        
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
            DiagnosticBag.CLASS_MEMBER_WITH_THAT_NAME_ALREADY_DECLARED_CODE,
        };
        
        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void VariableCanShadowField()
    {
        var text =
            """
            namespace Project
            {
                class Program {
                    static function main(){
                    }
                    
                    field : int;
                    function FunctionName()
                    { 
                        var field : int;
                    }
                }
            }
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void VariableCanShadowMethod()
    {
        var text =
            """
            namespace Project
            {
                class Program { 
                    static function main(){
                    }
                    function FunctionName()
                    { 
                        var FunctionName : int;
                    }
                }
            }
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }
    
    [Fact]
    public void VariableCanShadowClass()
    {
        var text =
            """
            namespace Project
            {
                class Program { 
                    static function main(){
                    }
                    function FunctionName()
                    { 
                        var TypeName : int;
                    }
                }
            } 
            """ ;
        
        var diagnostics = Array.Empty<string>();

        TestTools.AssertDiagnostics(text, diagnostics, Output);
    }

}