using System.Text;
using Xunit.Abstractions;

namespace TestProject1;

public class XUnitTextWriter : TextWriter
{
    public ITestOutputHelper Output { get; }
    public XUnitTextWriter(ITestOutputHelper output)
    {
        this.Output = output;
    }

    public override void WriteLine(string? text) 
        => this.Output.WriteLine(text);

    public override Encoding Encoding => Encoding.Unicode;
}