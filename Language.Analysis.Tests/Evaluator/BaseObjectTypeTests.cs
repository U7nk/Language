using FluentAssertions;
using Language.Analysis.CodeAnalysis.Symbols;
using Xunit.Abstractions;

namespace Language.Analysis.Tests.Evaluator;

public class BaseObjectTypeTests 
{
    public ITestOutputHelper TestOutputHelper { get; }

    public BaseObjectTypeTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void ClassCanBeImplicitlyCastedToObject()
    {
        var code = """  
            class MyClass
            {    
            }
            
            class Program
            {
                
                static function main()
                {
                    let myClass = new MyClass();
                    let obj : object = myClass;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NullGuard();
        result.Type.Name.Should().Be("MyClass");
        result.Type.BaseType.NullGuard().Name.Should().Be("object");
    }
    
    [Fact]
    public void ClassCanExplicitlyInheritFromObject()
    {
        var code = """  
            class MyClass : object
            {
            }
            
            class Program
            {  
                
                static function main()
                {
                    let myClass = new MyClass();
                    let obj : object = myClass;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NullGuard();
        result.Type.Name.Should().Be("MyClass");
        result.Type.BaseType.NullGuard().Name.Should().Be("object");
    }
    
    [Fact]
    public void ObjectsEqualsOperatorComparesSameInstanceByReferenceReturnsTrue()
    {
        var code = """  
            class MyClass : object
            {
            }
            
            class Program
            {   
                static function main()
                {
                    let myClass = new MyClass();
                    let myClassSame : object = myClass;
                    let refEquality = myClassSame == myClass;
                    
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NullGuard();
        result.Type.Name.Should().Be(BuiltInTypeSymbols.Bool.Name);
        result.LiteralValue.Should().Be(true);
    }
    
    [Fact]
    public void ObjectsEqualsOperatorComparesDifferentInstancesByReferenceReturnsFalse()
    {
        var code = """  
            class MyClass : object
            {
            }
            
            class Program
            {   
                static function main()
                {
                    let myClass = new MyClass();
                    let myClassSame = new MyClass();
                    let refEquality = myClassSame == myClass;
                }
            }
            """;
        
        var (result, diagnostics) = TestTools.Evaluate(code);
        diagnostics.Should().BeEmpty();
        result.NullGuard();
        result.Type.Name.Should().Be(BuiltInTypeSymbols.Bool.Name);
        result.LiteralValue.Should().Be(false);
    }
}
