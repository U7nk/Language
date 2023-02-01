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
    /// <summary>
    /// Doesn't work when diagnostics are from different SourceText's
    /// </summary>
    /// <param name="diagnostics"></param>
    /// <returns></returns>
    static string GetEnumeratedTextWithDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        var firstDiagnostic = diagnostics.FirstOrDefault().NullGuard();
        var sourceText = firstDiagnostic.TextLocation.Text.ToString();
        var lines = sourceText.Split(Environment.NewLine)
            .Select(x=> new {IsError = false, Text = x})
            .ToList();

        var inserts = new List<(int Index, string Text)>();
        foreach (var diagnostic in diagnostics)
        {
            inserts.Add((diagnostic.TextLocation.StartLine + 1, $"Error: {diagnostic.Message}"));
            var startLine  = lines[diagnostic.TextLocation.StartLine];
            var endLine = lines[diagnostic.TextLocation.EndLine];
            if (startLine == endLine)
            {
                var line = lines[diagnostic.TextLocation.StartLine];
                var start = diagnostic.TextLocation.StartCharacter;
                var end = diagnostic.TextLocation.EndCharacter;
                var newLine = line.Text.Insert(start, "[");
                newLine = newLine.Insert(end + 1, "]");
                lines[diagnostic.TextLocation.StartLine] = new {IsError = false, Text = newLine};
            }
            else
            {
                startLine = new
                {
                    IsError = false,
                    Text = startLine.Text[..diagnostic.TextLocation.StartCharacter] + "[" +
                           startLine.Text[diagnostic.TextLocation.StartCharacter..]
                };
                endLine = new 
                {
                    IsError = false,
                    Text = endLine.Text[..diagnostic.TextLocation.EndCharacter] + "]" + endLine.Text[diagnostic.TextLocation.EndCharacter..]
                };
                lines[diagnostic.TextLocation.StartLine] = startLine;
                lines[diagnostic.TextLocation.EndLine] = endLine;
            }
        }

        var insertedLines = 0;
        foreach (var insert in inserts)
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
        
        sourceText = string.Join(Environment.NewLine, enumeratedLines);
        return sourceText;
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

    public static void AssertDiagnostics(string text, bool isScript, string[] diagnosticsCodes, ITestOutputHelper output)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.RawText);
        var compilation = isScript 
            ? Compilation.CreateScript(null, syntaxTree)
            : Compilation.Create(syntaxTree);
        
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        var actualDiagnosticsTexts = diagnostics
            .Select(d => AnnotatedTextFromDiagnostic(d) + "\n" + d.Code + d.Message)
            .ToArray();

        var expectedDiagnosticsTexts = new List<string>();
        foreach (var i in 0..diagnosticsCodes.Length)
        {
            var code = diagnosticsCodes[i];
            var span = annotatedText.Spans[i];
            var t = AnnotatedText.Annotate(annotatedText.RawText, new []{ span });
            expectedDiagnosticsTexts.Add(t + "\n" + code);
        }
        
        if (diagnostics.Length != diagnosticsCodes.Length)
        {
            if (diagnostics.Any())
            {
                var actualText = GetEnumeratedTextWithDiagnostics(diagnostics);
                Assert.Fail($"Expected {diagnosticsCodes.Length} diagnostics, but got {diagnostics.Length}.\n" +
                            $"Expected: \n{string.Join(",\n", expectedDiagnosticsTexts)}\n\n" +
                            $"{new string('=', 69)}" + $"{Environment.NewLine}{Environment.NewLine}" +
                            $"Actual: {Environment.NewLine}" +
                            $"{actualText}");
            }

            diagnostics.Length.Should().Be(diagnosticsCodes.Length);
        }
        
        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        output.WriteLine("Spans:");
        output.WriteLine(AnnotatedText.Annotate(annotatedText.RawText, diagnostics.Select(x=> x.TextLocation.Span)));
        foreach (var i in 0..diagnosticsCodes.Length)
        {
            var expectedCode = diagnosticsCodes[i];
            var expectedSpan = annotatedText.Spans[i];
            
            var diagnostic = diagnostics.FirstOrDefault(x=> x.TextLocation.Span == expectedSpan && x.Code == expectedCode);
            if (diagnostic == null)
            {
                var diagnosticsWithSameSpan = diagnostics.Where(x=> x.TextLocation.Span == expectedSpan).ToArray();
                if (diagnosticsWithSameSpan.Length != 0)
                {
                    Assert.Fail($"Expected diagnostic with code {expectedCode} at {expectedSpan}, but got {diagnosticsWithSameSpan.Length} with different codes:\n" +
                                $"{string.Join(",\n", diagnosticsWithSameSpan.Select(x=> x.Code + x.Message))}");
                }
                
                var diagnosticsWithSameCode = diagnostics.Where(x=> x.Code == expectedCode).ToArray();
                if (diagnosticsWithSameCode.Length != 0)
                {
                    Assert.Fail($"Expected diagnostic with code {expectedCode} at {expectedSpan}, but got {diagnosticsWithSameCode.Length} with different spans:\n" +
                                $"{string.Join(",\n", diagnosticsWithSameCode.Select(x=> x.TextLocation.Span + x.Message))}");
                }
                
                Assert.Fail($"Expected diagnostic with code {expectedCode} at {expectedSpan}, but got none.\n" +
                            $"Expected: \n{string.Join(",\n", diagnosticsCodes)}\n" +
                            $"Actual: \n{string.Join(",\n", actualDiagnosticsTexts)}");
            }
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
        if (context is ContextType.TopLevelStatement)
        {
            return content;
        }

        if (context is ContextType.Method)
        {

            var function = $$"""
                class Program
                {
                    InstanceField : int;
                    static StaticField : int;
                    
                    static function main()
                    {
                        {{ content.ReplaceLineEndings("\n        ") }} 
                    }
                }
                """ ;
            return function;
        }

        if (context is ContextType.TopLevelMethod)
        {
            var function = $$"""
                function topLevelMethod()
                {
                    {{ content.ReplaceLineEndings("\n    ") }} 
                }
                topLevelMethod();
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
        /// function topLevelMethod()
        /// {
        ///     <b>*input statements*</b>
        /// }
        /// </code>
        /// </example>
        /// </summary>
        TopLevelMethod,
        
        /// <summary>
        /// <example>
        /// <code>
        /// <b>*input statements*</b>
        /// </code>
        /// </example>
        /// </summary>
        TopLevelStatement,
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
                result.Add(BuiltInTypeSymbols.Int);
            }
            else if (symbolType == typeof(ParameterSymbol))
            {
                result.Add(new ParameterSymbol(Option.None, "parameter", null, BuiltInTypeSymbols.Int));
            }
            else if (symbolType == typeof(VariableSymbol))
            {
                result.Add(new VariableSymbol(Option.None, "variable", null, BuiltInTypeSymbols.Int, false ));
            }
            else if (symbolType == typeof(FieldSymbol))
            {
                result.Add(new FieldSymbol(Option.None, false, "field",  null!, BuiltInTypeSymbols.Int));
            }
        }

        return result;
    }
}