using FluentAssertions;
using Language.Analysis.Extensions;

namespace Language.Analysis.Tests;

public class Extensions
{
    // write xunit fact test that checks foreach loop
    [Fact]
    public void ForwardRangeIteratorTest()
    {
        var expected = "";
        foreach (var i in Enumerable.Range(0, 16))
        {
            expected += i;
        }

        var real = "";
        foreach (var i in 0..16)
        {
            real += i;
        }

        real.Should().Be(expected);
    }
    
    // write backward range iterator test
    [Fact]
    public void BackwardRangeIteratorTest()
    {
        var expected = "";
        foreach (var i in Enumerable.Range(0, 16).Reverse())
        {
            expected += i;
        }

        var real = "";
        foreach (var i in 16..0)
        {
            real += i;
        }

        real.Should().Be(expected);
    }
    
    
    
}