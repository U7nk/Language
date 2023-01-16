using FluentAssertions;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.Evaluator;

public class InheritanceTests 
{
    readonly ITestOutputHelper _output;

    public InheritanceTests(ITestOutputHelper output)
    {
        _output = output;
    }
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

        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
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
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
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

        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
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
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
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
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
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
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(1);
    }


    [Fact]
    public void DerivedClassOverridingVirtualMethodReturnsDifferentResults()
    {
        var code =  $$""""
                    class Base
                    {
                        function virtual MyMethod() : string
                        {
                            return "MyMethod";
                        }
                    }
                    
                    class Inheritor : Base
                    {
                        function override MyMethod() : string
                        {
                            return "OurMethod!";
                        }
                    }
                    class Program
                    {
                        static function main()
                        {
                            let baseInstance = new Base();
                            let inheritor = new Inheritor();
                            var featureIsWorking = false;
                            if baseInstance.MyMethod() != inheritor.MyMethod() 
                            {
                                featureIsWorking = true;
                            }
                            featureIsWorking = featureIsWorking;
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void DerivedClassOverridingVirtualMethodReturnsDifferentResultsEvenIfCastedToBase()
    {
        var code =  $$""""
                    class Base
                    {
                        function virtual MyMethod() : string
                        {
                            return "MyMethod";
                        }
                    }
                    
                    class Inheritor : Base
                    {
                        function override MyMethod() : string
                        {
                            return "OurMethod!";
                        }
                    }
                    class Program
                    {
                        static function main()
                        {
                            let baseInstance = new Base();
                            let inheritor = (Base)new Inheritor();
                            var featureIsWorking = false;
                            if baseInstance.MyMethod() != inheritor.MyMethod() 
                            {
                                featureIsWorking = true;
                            }
                            featureIsWorking = featureIsWorking;
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code).IfErrorOutputDiagnosticsAndThrow(_output);
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void ClassCanDeriveFromTwoDifferentClasses()
    {
        var code = $$""""
                    class BaseOne
                    {
                        function MethodOne() : string
                        {
                            return "Base1";
                        }
                    }

                    class BaseTwo
                    {
                        function MethodTwo() : string
                        {
                            return "Base2";
                        }
                    }
                    
                    class Inheritor : BaseOne, BaseTwo
                    {
                    
                    }

                    class Program
                    {
                        static function main()
                        {
                            let inheritor = new Inheritor();
                            var featureIsWorking = false;
                            if inheritor.MethodOne() == "Base1" && inheritor.MethodTwo() == "Base2"
                            {
                                featureIsWorking = true;
                            }
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code).IfErrorOutputDiagnosticsAndThrow(_output);

        result.IsOk.Should().BeTrue();
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void ClassCanOverrideVirtualMethodsWithMultipleInheritance()
    {
        var code = $$""""
                    class BaseOne
                    {
                        function virtual MethodOne() : string
                        {
                            return "Base1";
                        }
                    }

                    class BaseTwo
                    {
                        function virtual MethodTwo() : string
                        {
                            return "Base2";
                        }
                    }
                    
                    class Inheritor : BaseOne, BaseTwo
                    {
                        function override MethodOne() : string
                        {
                            return "Inheritor1";
                        }

                        function override MethodTwo() : string
                        {
                            return "Inheritor2";
                        }
                    }

                    class Program
                    {
                        static function main()
                        {
                            let inheritor = new Inheritor();
                            var featureIsWorking = false;
                            if inheritor.MethodOne() == "Inheritor1" && inheritor.MethodTwo() == "Inheritor2"
                            {
                                featureIsWorking = true;
                            }
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code).IfErrorOutputDiagnosticsAndThrow(_output);

        result.IsOk.Should().BeTrue();
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void TypeCanBeCastedToAnyOfBaseTypesWhenMultipleInheritance()
    {
        var code = $$""""
                    class BaseOne
                    {
                        function MethodOne() : string
                        {
                            return "Base1";
                        }
                    }

                    class BaseTwo
                    {
                        function MethodTwo() : string
                        {
                            return "Base2";
                        }
                    }
                    
                    class Inheritor : BaseOne, BaseTwo
                    {
                    }

                    class Program
                    {
                        static function main()
                        {
                            let inheritor = new Inheritor();
                            let inheritorAsBaseOne = (BaseOne)inheritor;
                            let inheritorAsBaseTwo = (BaseTwo)inheritor;

                            var featureIsWorking = false;
                            if inheritorAsBaseOne.MethodOne() == "Base1" && inheritorAsBaseTwo.MethodTwo() == "Base2"
                            {
                                featureIsWorking = true;
                            }
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code).IfErrorOutputDiagnosticsAndThrow(_output);

        result.IsOk.Should().BeTrue();
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void TypeCanBeCastedToAnyOfBaseTypesAndOverrideMethodsReturningOverridingValueWhenMultipleInheritance()
    {
        var code = $$""""
                    class BaseOne
                    {
                        function virtual MethodOne() : string
                        {
                            return "Base1";
                        }
                    }

                    class BaseTwo
                    {
                        function virtual MethodTwo() : string
                        {
                            return "Base2";
                        }
                    }
                    
                    class Inheritor : BaseOne, BaseTwo
                    {
                        function override MethodOne() : string
                        {
                            return "Inheritor1";
                        }

                        function override MethodTwo() : string
                        {
                            return "Inheritor2";
                        }
                    }

                    class Program
                    {
                        static function main()
                        {
                            let inheritor = new Inheritor();
                            let inheritorAsBaseOne = (BaseOne)inheritor;
                            let inheritorAsBaseTwo = (BaseTwo)inheritor;

                            var featureIsWorking = false;
                            if inheritorAsBaseOne.MethodOne() == "Inheritor1" && inheritorAsBaseTwo.MethodTwo() == "Inheritor2"
                            {
                                featureIsWorking = true;
                            }
                        }
                    }
                    """";
        
        var result = TestTools.Evaluate(code).IfErrorOutputDiagnosticsAndThrow(_output);

        result.IsOk.Should().BeTrue();
        result.Ok.NullGuard();
        result.Ok.LiteralValue.Should().Be(true);
    }
}