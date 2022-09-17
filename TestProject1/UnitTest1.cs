using FluentAssertions;
using FluentAssertions.Common;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;
using Xunit.Abstractions;
namespace TestProject1;

public class UnitTest1
{
    readonly XUnitTextWriter _output;

    public UnitTest1(ITestOutputHelper output)
    {
        this._output = new XUnitTextWriter(output);
    }
    
    [Fact]
    public void Evaluate()
    {
        
        _output.WriteLine("Result: " + Build($$"""
            {  
                var b = 1; 
                var i = 5; 
                for (i = 1; i < 4; i = i + 1)
                {
                   b = i;
                } 
                b;
            }
            """));
    }

    object Build(string input)
    {
        var syntaxTree = SyntaxTree.Parse(input);
        var compilation = new Compilation(syntaxTree);
        
        _output.WriteLine("Syntax Tree:");
        compilation.SyntaxTree.Root.WriteTo(_output);
        _output.WriteLine();
        _output.WriteLine();
        _output.WriteLine("Bound Tree:");
        var output = new StringWriter();
        compilation.EmitTree(output);
        output.ToString().Split(Environment.NewLine)
            .ToList()
            .ForEach(x => _output.WriteLine(x));
        
        var variables = new Dictionary<VariableSymbol, object?>();
        var evaluation = compilation.Evaluate(variables);
        if (evaluation.Diagnostics.Any())
        {
            foreach (var diagnostic in evaluation.Diagnostics)
            {
                var text = syntaxTree.SourceText;
                var lineIndex = text.GetLineIndex(diagnostic.Span.Start); 
                var lineNumber = lineIndex + 1; 
                var prefix = input.Substring(0, diagnostic.Span.Start);
                var error = input.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                var suffix = input.Substring(diagnostic.Span.End);
                var line = $"({lineNumber},{diagnostic.Span.Start - text.Lines[lineIndex].Start + 1}): \"{prefix}> {error} <{suffix}\"";
                _output.WriteLine(line);
                _output.WriteLine(diagnostic.Message);
            }
        }
        else
        {
            return evaluation.Result;
        }

        return evaluation.Diagnostics;
    }
}