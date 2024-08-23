using System.Collections.Immutable;
using FluentAssertions;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;
using Xunit.Abstractions;

namespace Language.Analysis.Tests;

public static class TestTools
{
    static string GetEnumeratedTextWithDiagnostics(string sourceText,
                                                   IList<(TextSpan Span, string Code)> diagnostics)
    {
        var st = SourceText.From(sourceText);
        ;
        return GetEnumeratedTextWithDiagnostics(diagnostics.Select(x => new Diagnostic(new TextLocation(st, x.Span), x.Code, x.Code)).ToImmutableArray());
    }

    /// <summary>
    /// Doesn't work when diagnostics are from different SourceText's
    /// </summary>
    /// <param name="diagnostics"></param>
    /// <returns></returns>
    static string GetEnumeratedTextWithDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        var firstDiagnostic = diagnostics.FirstOrDefault().NullGuard();
        var sourceText = firstDiagnostic.TextLocation.Text;
        var lines = sourceText.Lines
            .Select(x=> new {IsError = false, Text = x.ToString()})
            .ToList();

        var lineInserts = new Dictionary<int, List<(int index, bool isOpen)>>();
        for (var index = 0; index < lines.Count; index++) 
            lineInserts.Add(index, new List<(int index, bool isOpen)>());

        var inserts = new List<(int Index, string Text, Diagnostic diagnostic)>();
        
        foreach (var diagnostic in diagnostics)
        {
            inserts.Add((diagnostic.TextLocation.EndLine + 1, $"Error: {diagnostic.Message}", diagnostic));
            lineInserts[diagnostic.TextLocation.StartLine].Add((diagnostic.TextLocation.StartCharacter, true));
            lineInserts[diagnostic.TextLocation.EndLine].Add((diagnostic.TextLocation.EndCharacter, false));
        }

        foreach (var lineInsert in lineInserts)
        {
            var line = lines[lineInsert.Key];
            var list = lineInsert.Value.OrderBy(x => x.index).ToList();
            var newLine = line.Text;
            var shift = 0;
            foreach (var insert in list)
            {
                newLine = newLine.Insert(insert.index + shift, insert.isOpen ? "[" : "]");
                shift++;
            }

            lines[lineInsert.Key] = new { IsError = false, Text = newLine };
        }
        
        var insertedLines = 0;
        foreach (var insert in inserts.OrderBy(x=> x.Index).ThenBy(x=> x.diagnostic.TextLocation.EndCharacter))
        {
            lines.Insert(insert.Index + insertedLines, new {IsError = true, Text = insert.Text});
            insertedLines++;
        }
        // enumerate lines
        var lineCounter = 0;
        var enumeratedLines = lines.Select(line =>
        {
            if (line.IsError)
                return line.Text;
            
            var newText = $"{lineCounter}: {line.Text}";
            lineCounter++;
            return newText;
        }).ToList();
        
        return string.Join("\n", enumeratedLines);
    }
    public static Result<ObjectInstance?, ImmutableArray<Diagnostic>> AssertNoDiagnostics(this Result<ObjectInstance?, ImmutableArray<Diagnostic>> result, ITestOutputHelper output)
    {
        if (result.IsOk)
            return result;
        
        var diagnostics = result.Error;
        var sourceText = GetEnumeratedTextWithDiagnostics(diagnostics);
        output.WriteLine(sourceText);
        throw new InvalidOperationException("Compilation failed");
    }

    public static Result<ObjectInstance?, ImmutableArray<Diagnostic>> Evaluate(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        if (syntaxTree.Diagnostics.Any())
            return syntaxTree.Diagnostics.ToImmutableArray();
        
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();
        if (diagnostics.Length > 0)
        {
            return result.Diagnostics;
        }
        
        return result.Result;
    }
    
    public static void AssertDiagnosticsWithMessages(string text, string[] expectedDiagnosticTexts)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.RawText);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        var actualDiagnosticTexts = diagnostics
            .Select(d => AnnotatedTextFromDiagnostic(d) + "\n" + d.Code + d.Message)
            .ToArray();
        if (diagnostics.Length != expectedDiagnosticTexts.Length)
        {
            Assert.Fail($"Expected {expectedDiagnosticTexts.Length} diagnostics, but got {diagnostics.Length}.\n" +
                        $"Expected: \n{string.Join(",\n", expectedDiagnosticTexts)}\n" +
                        $"Actual: \n{string.Join(",\n", actualDiagnosticTexts)}");
            diagnostics.Length.Should().Be(expectedDiagnosticTexts.Length);
        }
        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        foreach (var i in ..expectedDiagnosticTexts.Length)
        {
            var expectedMessage = expectedDiagnosticTexts[i];
            var actualMessage = diagnostics[i].Message;
            actualMessage.Should().Be(expectedMessage, "Diagnostic messages do not match");


            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].TextLocation.Span;
            actualSpan.Should().BeOfType<TextSpan>().And.Be(expectedSpan, "Diagnostic spans do not match");
        }
    }

    public static void AssertDiagnostics(string text, string[] diagnosticsCodes, ITestOutputHelper output)
    {
        var annotatedText = AnnotatedText.Parse(text);
        diagnosticsCodes.Length.Should().Be(annotatedText.Spans.Length, "Must mark as many spans as there expected diagnostics");
        
        var syntaxTree = SyntaxTree.Parse(annotatedText.RawText);
        var compilation = Compilation.Create(syntaxTree);
        
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics;

        var actualDiagnosticsTexts = diagnostics
            .Select(d => AnnotatedTextFromDiagnostic(d) + "\n" + d.Code + d.Message)
            .ToArray();

        var expectedDiagnostics = new List<Diagnostic>();
        foreach (var i in 0..diagnosticsCodes.Length)
        {
            var code = diagnosticsCodes[i];
            var span = annotatedText.Spans[i];
            new Diagnostic(new TextLocation(syntaxTree.SourceText, span), code, code).AddTo(expectedDiagnostics);
        }
        
        if (diagnostics.Length != diagnosticsCodes.Length)
        {
            if (diagnostics.Any())
            {
                var actualText = GetEnumeratedTextWithDiagnostics(diagnostics);
                Assert.Fail($"Expected {diagnosticsCodes.Length} diagnostics, but got {diagnostics.Length}.\n" +
                            $"Expected: \n{GetEnumeratedTextWithDiagnostics(expectedDiagnostics.ToImmutableArray())}\n\n" +
                            $"{new string('=', 69)}" + $"{Environment.NewLine}{Environment.NewLine}" +
                            $"Actual: {Environment.NewLine}" +
                            $"{actualText}");
            }
            else
            {
                Assert.Fail($"Expected {diagnosticsCodes.Length} diagnostics, but got {diagnostics.Length}.\n" +
                            $"Expected: \n{GetEnumeratedTextWithDiagnostics(expectedDiagnostics.ToImmutableArray())}\n\n" +
                            $"{new string('=', 69)}" + $"{Environment.NewLine}{Environment.NewLine}");
            }
        }
        
        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        var gs = diagnostics.ToList();
        foreach (var expectedDiagnostic in expectedDiagnostics)
        {
            var sx = gs.FirstOrDefault(x => x.TextLocation == expectedDiagnostic.TextLocation && x.Code == expectedDiagnostic.Code);
            if (sx is null)
            {
                var actualText = GetEnumeratedTextWithDiagnostics(diagnostics);
                Assert.Fail($"Expected {diagnosticsCodes.Length} diagnostics, but got {diagnostics.Length}.\n" +
                            $"Expected: \n{GetEnumeratedTextWithDiagnostics(expectedDiagnostics.ToImmutableArray())}\n\n" +
                            $"{new string('=', 69)}" + $"{Environment.NewLine}{Environment.NewLine}" +
                            $"Actual: {Environment.NewLine}" +
                            $"{actualText}");
            }
            gs.Remove(sx);
        }
    }

    public static string AnnotatedTextFromDiagnostic(Diagnostic diagnostic)
    {
        var pre = diagnostic.TextLocation.Text.ToString(0, diagnostic.TextLocation.Span.Start);
        var marked = "[" + diagnostic.TextLocation.Text.ToString(diagnostic.TextLocation.Span) + "]";
        var post = diagnostic.TextLocation.Text.ToString(
            diagnostic.TextLocation.Span.Start + marked.Length - 2, 
            diagnostic.TextLocation.Text.Length - diagnostic.TextLocation.Span.Start - marked.Length + 2);
        
        return pre + marked + post;
    }

    public static string StatementsInContext(string content, ContextType context)
    {
        if (context is ContextType.Method)
        {

            var function = $$"""
                namespace MyProgram
                {
                    class Program
                    {
                        InstanceField : int;
                        static StaticField : int;
                        
                        static function main()
                        {
                            {{ content.ReplaceLineEndings("\n        ") }} 
                        }
                    }
                }
                """ ;
            return function;
        }
        
        throw new Exception("Unknown context");
    }

    public enum ContextType
    {
        /// <summary>
        /// <example>
        /// <code>
        /// class Program
        /// {
        ///     Field : int;
        /// 
        ///     function main()
        ///     {
        ///         <b>*input statements*</b>
        ///     }
        /// }
        /// </code>
        /// </example>
        /// </summary>
        Method,
    }

    public static IEnumerable<object[]> AllContextTypesForStatements()
    {
        foreach (var contextType in Enum.GetValues(typeof(ContextType)))
        {
            yield return new[] { contextType };
        }
    }

    /// <summary>
    /// Doesn't include abstract types
    /// </summary>
    /// <returns></returns>
    public static List<Type> GetAllKindsOfSymbolTypes()
    {
        var symbolTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(t => t.IsSubclassOf(typeof(Symbol)) && t.IsAbstract is false))
            .ToList();

        return symbolTypes;
    }
    
    public static List<Symbol> GetAllKindsOfSymbols()
    {
        var result = new List<Symbol>();
        var symbolTypes = GetAllKindsOfSymbolTypes();
        foreach (var symbolType in symbolTypes)
        {
            if (symbolType == typeof(TypeSymbol))
            {
                result.Add(TypeSymbol.BuiltIn.Int());
            }
            else if (symbolType == typeof(ParameterSymbol))
            {
                result.Add(new ParameterSymbol(Option.None, "parameter", TypeSymbol.BuiltIn.Int()));
            }
            else if (symbolType == typeof(VariableSymbol))
            {
                result.Add(new VariableSymbol(Option.None, "variable", TypeSymbol.BuiltIn.Int(), false ));
            }
            else if (symbolType == typeof(FieldSymbol))
            {
                result.Add(new FieldSymbol(Option.None, false, "field",  null!, TypeSymbol.BuiltIn.Int()));
            }
        }

        return result;
    }
}