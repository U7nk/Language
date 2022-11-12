using FluentAssertions;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.Tests.Text;

public class SourceTextTests
{
  [Theory]
  [InlineData(".", 1)]
  [InlineData(".\r\n", 2)]
  [InlineData(".\r\n\r\n", 3)]
  public void SourceText_IncludesLastLine(string text, int expectedLineCount)
  {
    var sourceText = SourceText.From(text);
    sourceText.Lines.Length.Should().Be(expectedLineCount);
  }
}