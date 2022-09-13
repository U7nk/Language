using System.Text;
using Xunit.Abstractions;

namespace TestProject1;

public class XUnitTextWriter : TextWriter
{
    public ITestOutputHelper Output { get; }
    public XUnitTextWriter(ITestOutputHelper output)
    {
        Output = output;
    }

    public override void WriteLine(string? text) 
        => Output.WriteLine(text);

    public override Encoding Encoding => Encoding.Unicode;
}