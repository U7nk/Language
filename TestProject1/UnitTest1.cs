using FluentAssertions;
using FluentAssertions.Common;
using Xunit.Abstractions;


namespace TestProject1;
using Wired;
public class UnitTest1
{
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Test()
    {
        const string input = "5 + 2 * (3)";
        var lexer = new Lexer(input);
        this.output.WriteLine(
            string.Join(
                "\n",
                lexer.Parse().Select(x => x.Kind + " : " + x.Text)));
    }
}