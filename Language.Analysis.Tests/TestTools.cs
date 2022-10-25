using System.Collections.Immutable;
using FluentAssertions;
using Language.Analysis;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;

namespace TestProject1;

public class TestTools
{
    static void AssertDiagnostics(string text, string[] diagnosticsText)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        diagnostics.Length.Should().Be(diagnosticsText.Length);

        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        foreach (var i in 0..diagnosticsText.Length)
        {
            var expectedMessage = diagnosticsText[i];
            var actualMessage = diagnostics[i].Message;
            actualMessage.Should().Be(expectedMessage, "Diagnostic messages do not match");


            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].TextLocation.Span;
            actualSpan.Should().BeOfType<TextSpan>().And.Be(expectedSpan, "Diagnostic spans do not match");
        }
    }

    internal static void AssertDiagnosticsWithTimeout(string text, string[] diagnosticsText)
    {
        AssertDiagnostics(text, diagnosticsText);
    }
}