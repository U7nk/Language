using FluentAssertions;
using Language.Analysis;

namespace TestProject1.Evaluator;

public class InheritanceTests 
{
    [Fact]
    public void MethodDeclaredInBaseClassCanBeCalledOnDerivedClass()
    {
        var code = """
            class Base
            {
                function BaseMethod() : int
                {
                    return 1;
                }
            }

            class Derived : Base
            { 
            }

            class Program
            {
                static function main()
                {
                    var d = new Derived();
                    d.BaseMethod();
                }
            }
            """;

        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
    
    [Fact]
    public void FieldDeclaredInBaseClassCanBeCalledOnDerivedClass()
    {
        var code = """
            class Base
            {
                BaseField : int;
            }

            class Derived : Base
            { 
            }

            class Program
            {
                static function main()
                {
                    var d = new Derived();
                    d.BaseField = 1;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
    
    [Fact]
    public void MethodDeclaredInBaseClassCanBeCalledOnTwoLevelDerivedClass()
    {
        var code = """
            class Base
            {
                function BaseMethod() : int
                {
                    return 1;
                }
            }

            class DerivedFirst : Base
            { 
            }
            
            class DerivedSecond : DerivedFirst
            { 
            }

            class Program
            {
                static function main()
                {
                    var d = new DerivedSecond();
                    d.BaseMethod();
                }
            }
            """;

        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
    
    [Fact]
    public void FieldDeclaredInBaseClassCanBeCalledOnTwoLevelDerivedClass()
    {
        var code = """
            class Base
            { 
                BaseField : int;
            }

            class DerivedFirst : Base
            { 
            }
            
            class DerivedSecond : DerivedFirst
            { 
            }

            class Program
            {
                static function main()
                {
                    var d = new DerivedSecond();
                    d.BaseField = 1;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
    
    [Fact]
    public void DerivedTypeCanBeDeclaredAsBase()
    {
        var code = """
            class Base
            { 
                BaseField : int;
            }

            class Derived : Base
            { 
            }
            

            class Program
            {
                static function main()
                {
                    var d : Base = new Derived();
                    d.BaseField = 1;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
    
    [Fact]
    public void DerivedTypeCanBeAssignedToBase()
    {
        var code = """
            class Base
            { 
                BaseField : int;
            }

            class Derived : Base
            { 
            }
            

            class Program
            {
                static function main()
                {
                    var baseInstance : Base;
                    baseInstance = new Derived();
                    baseInstance.BaseField = 1;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NG().LiteralValue.Should().Be(1);
    }
}