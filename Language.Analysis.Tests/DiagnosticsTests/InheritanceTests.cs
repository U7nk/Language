using Language.Analysis.CodeAnalysis;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.DiagnosticsTests;

public class InheritanceTests
{
    public ITestOutputHelper Output { get; set; }
    public InheritanceTests(ITestOutputHelper output)
    {
        Output = output;
    }

    [Fact]
    public void DerivedMethodCannotBeCalledAfterConversionToBase()
    {
        var code = @"
            class Base
            {
            }

            class Derived : Base
            {
                function DerivedMethod() { }
            }

            class Program
            {
                static function main()
                {
                    var derived = new Derived();
                    var base : Base = derived;
                    base.[DerivedMethod]();   
                }
            }";
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_METHOD_CODE,
        };

        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }
    
    [Fact]
    public void DerivedMethodCannotBeCalledAfterDeclarationAsBase()
    {
        var code = @"
            class Base
            {
            }

            class Derived : Base
            {
                function DerivedMethod() { }
            }

            class Program
            {
                static function main()
                {
                    var base : Base = new Derived();
                    base.[DerivedMethod]();   
                }
            }";
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_METHOD_CODE,
        };
        
        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }
    
    [Fact]
    public void DeriveFieldCannotBeAccessedAfterConversionToBase()
    {
        var code = @"
            class Base
            {
            }

            class Derived : Base
            {
                DerivedField : int;
            }

            class Program
            {
                static function main()
                {
                    var derived = new Derived();
                    var casted : Base = derived;
                    let x = casted.[DerivedField];   
                }
            }";
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_FIELD_ACCESS_CODE,
        };

        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }
    
    [Fact]
    public void DerivedFieldCannotBeAccessedAfterDeclarationAsBase()
    {
        var code = @"
            class Base
            {
            }

            class Derived : Base
            {
                DerivedField : int;
            }

            class Program
            {
                static function main()
                {
                    var casted : Base = new Derived();
                    let x = casted.[DerivedField];   
                }
            }";
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_FIELD_ACCESS_CODE,
        };
        
        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }
    
    [Fact]
    public void ClassCannotInheritFromSelf()
    {
        var code = @"
            class [Base] : Base
            {
            }

            class Program
            {
                static function main()
                {
                }
            }";
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
        };
        
        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }

}