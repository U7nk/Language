using FluentAssertions;
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
        var code = """
            namespace Project
            {
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
                }
            }
            """;
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_METHOD_CODE,
        };

        TestTools.AssertDiagnostics(code, diagnostics, Output);
    }
    
    [Fact]
    public void DerivedMethodCannotBeCalledAfterDeclarationAsBase()
    {
        var code = """
            namespace Project
            {
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
                }
            }
            """;
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_METHOD_CODE,
        };
        
        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    
    [Fact]
    public void DeriveFieldCannotBeAccessedAfterConversionToBase()
    {
        var code = """
            namespace Project
            {
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
                }
            }
            """;
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_FIELD_ACCESS_CODE,
        };

        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    
    [Fact]
    public void DerivedFieldCannotBeAccessedAfterDeclarationAsBase()
    {
        var code = """
            namespace Project
            {
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
                }
            }
            """;
        var diagnostics = new[]
        {
            DiagnosticBag.UNDEFINED_FIELD_ACCESS_CODE,
        };
        
        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    
    [Fact]
    public void ClassCannotInheritFromSelf()
    {
        var code = """
            namespace Project
            {
                class [Base] : Base
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
        var diagnostics = new[]
        {
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
        };
        
        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    [Fact]
    public void ClassCannotInheritFromSelfThroughBaseClass()
    {
        var code = """
            namespace Project
            {
                class [Inheritor] : Base 
                {
                    function Method() : int
                    {
                        return 1;
                    }
                }
                class [Base] : Inheritor
                {
                    function BlabLa() { this.Method(); }
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
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
        };
        
        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    [Fact]
    public void ClassCannotInheritFromSelfThroughMultipleBaseClass()
    {
        var code = """
            namespace Project
            {
                class [Inheritor] : Base 
                {
                    function Method() : int
                    {
                        return 1;
                    }
                }
                class [SecondInheritor] : Inheritor 
                {
                }
                class [Base] : SecondInheritor
                {
                    function BlabLa() { this.Method(); }
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
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
            DiagnosticBag.CLASS_CANNOT_INHERIT_FROM_SELF_CODE,
        };
        
        TestTools.AssertDiagnostics(code,  diagnostics, Output);
    }
    
    [Fact]
    public void ClassDiamondProblemWithMethodsReported()
    {
        var code = """"
                    namespace Project
                    {
                        class BaseOne
                        {
                            function MethodOne() : string
                            {
                                return "Base1";
                            }
                        }
                        class BaseTwo
                        {
                            function MethodOne() : string
                            {
                                return "Base2";
                            }
                        }
                        class [Inheritor] : BaseOne, BaseTwo
                        {
                        }
                        class Program
                        {
                            static function main()
                            {
                            }
                        }
                    }
                    """";
        TestTools.AssertDiagnostics(code,  new []
        {
            DiagnosticBag.INHERITANCE_DIAMOND_PROBLEM_CODE,
        }, Output);
        var output = TestTools.Evaluate(code);
    }

    [Fact]
    public void ClassDiamondProblemWithMethodAndFieldReportsAmbiguity()
    {
        var code = """"
                    namespace Project
                    {   
                        class BaseOne
                        {
                            function MethodOne() : string
                            {
                                return "Base1";
                            }
                        }
                        class BaseTwo
                        {
                            MethodOne : string;
                        }
                        class [Inheritor] : BaseOne, BaseTwo
                        {
                        }
                        class Program
                        {
                            static function main()
                            {
                            }
                        }
                    }
                    """";
        
        TestTools.AssertDiagnostics(code,  new []
        {
            DiagnosticBag.INHERITANCE_DIAMOND_PROBLEM_CODE
        }, Output);
    }

    [Fact]
    public void ClassDiamondProblemWithFieldsReportsAmbiguity()
    {
        var code = """"
                    namespace Project
                    {
                        class BaseOne
                        {
                            FieldOne : string;
                        }
                        class BaseTwo
                        {
                            FieldOne : string;
                        }
                        class [Inheritor] : BaseOne, BaseTwo
                        {
                        }
                        class Program
                        {
                            static function main()
                            {
                            }
                        }
                    }
                    """";
        TestTools.AssertDiagnostics(code,  new []
        {
            DiagnosticBag.INHERITANCE_DIAMOND_PROBLEM_CODE,
        }, Output);
    }
    
    [Fact]
    public void ClassDiamondProblemWithFieldsReportsAmbiguityEvenThroughOneInheritanceLevel()
    {
        var code = """"
                    namespace Project
                    {
                        class BaseOne
                        {
                            FieldOne : string;
                        }
                        class BaseTwo : BaseOne
                        {
                        }
                        class BaseThree
                        {
                            FieldOne : string;
                        }
                        class [Inheritor] : BaseTwo, BaseThree
                        {
                        }
                        class Program
                        {
                            static function main()
                            {
                            }
                        }
                    }
                    """";
        TestTools.AssertDiagnostics(code,  new []
        {
            DiagnosticBag.INHERITANCE_DIAMOND_PROBLEM_CODE,
        }, Output);
    }
    [Fact]
    public void ClassDiamondProblemWithFieldsReportsAmbiguityEvenThroughTwoInheritanceLevel()
    {
        var code = """"
                    namespace Project
                    {
                        class BaseOne
                        {
                            FieldOne : string;
                        }
                        class BaseTwo : BaseOne
                        {
                        }
                        class BaseThree : BaseTwo { }
                        class BaseFour 
                        {
                            FieldOne : string;
                        }
                        class BaseFive : BaseFour { }
                        class [Inheritor] : BaseFive, BaseThree
                        {
                        }
                        class Program
                        {
                            static function main()
                            {
                            }
                        }
                    }
                    """";
        TestTools.AssertDiagnostics(code,  new []
        {
            DiagnosticBag.INHERITANCE_DIAMOND_PROBLEM_CODE,
        }, Output);
    }
}