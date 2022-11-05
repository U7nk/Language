using System.CodeDom.Compiler;
using System.Collections.Immutable;
using FluentAssertions;
using Language.Analysis;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace TestProject1;

public class TestTools
{

    public static (EvaluationResult result, ImmutableArray<Diagnostic> diagnostics) Evaluate(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();
        return (result, diagnostics);
    }
    
    public static void AssertDiagnosticsWithMessages(string text, string[] expectedDiagnosticTexts)
    {
        var annotatedText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
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

        foreach (var i in 0..expectedDiagnosticTexts.Length)
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
        var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        var compilation = Compilation.Create(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, ObjectInstance?>());
        var diagnostics = result.Diagnostics.ToImmutableArray();

        var diagnosticsTexts = diagnostics
            .Select(d => AnnotatedTextFromDiagnostic(d) + "\n" + d.Code + d.Message)
            .ToArray();

        if (diagnostics.Length != diagnosticsCodes.Length)
        {
            Assert.Fail($"Expected {diagnosticsCodes.Length} diagnostics, but got {diagnostics.Length}.\n" +
                        $"Expected: \n{string.Join(",\n", diagnosticsCodes)}\n" +
                        $"Actual: \n{string.Join(",\n", diagnosticsTexts)}");
            diagnostics.Length.Should().Be(diagnosticsCodes.Length);
        }
        
        diagnostics.Length.Should().Be(
            annotatedText.Spans.Length,
            "Must mark as many spans as there expected diagnostics");

        foreach (var i in 0..diagnosticsCodes.Length)
        {
            var expectedCode = diagnosticsCodes[i];
            var actualCode = diagnostics[i].Code;
            actualCode.Should().Be(expectedCode, "Diagnostic messages do not match");


            var expectedSpan = annotatedText.Spans[i];
            var actualSpan = diagnostics[i].TextLocation.Span;
            actualSpan.Should().BeOfType<TextSpan>().And.Be(expectedSpan, "Diagnostic spans do not match");
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
                result.Add(TypeSymbol.Int);
            }
            else if (symbolType == typeof(ParameterSymbol))
            {
                result.Add(new ParameterSymbol(ImmutableArray<SyntaxNode>.Empty, "parameter", null, TypeSymbol.Int));
            }
            else if (symbolType == typeof(VariableSymbol))
            {
                result.Add(new VariableSymbol(ImmutableArray<SyntaxNode>.Empty, "variable", null, TypeSymbol.Int, false ));
            }
            else if (symbolType == typeof(FieldSymbol))
            {
                result.Add(new FieldSymbol(ImmutableArray<SyntaxNode>.Empty, false, "field",  null!, TypeSymbol.Int));
            }
        }

        return result;
    }
}