using FluentAssertions;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;
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
        
        var result = TestTools.Evaluate(code);
        result.IsOk.Should().BeTrue();
        result.Ok.NullGuard();
        
        (result.Ok is 
        {
            Type.Name: "MyClass",
            Type.BaseTypes.Count: 1,
        }).Should().BeTrue();
        result.Ok.Type.BaseTypes.Single().Name.Should().Be("object");
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
        
        var result = TestTools.Evaluate(code);
        (result.Ok is 
        {
            Type.Name: "MyClass",
            Type.BaseTypes.Count: 1
        }).Should().BeTrue();
        result.Ok.NullGuard().Type.BaseTypes.Single().Name.Should().Be("object");
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
        
        var result = TestTools.Evaluate(code);
        result.Ok.NullGuard();
        result.Ok.Type.Name.Should().Be(BuiltInTypeSymbols.Bool.Name);
        result.Ok.LiteralValue.Should().Be(true);
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
        
        var result = TestTools.Evaluate(code);
        
        result.Ok.NullGuard();
        result.Ok.Type.Name.Should().Be(BuiltInTypeSymbols.Bool.Name);
        result.Ok.LiteralValue.Should().Be(false);
    }
    
}
