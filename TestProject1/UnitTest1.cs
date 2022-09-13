using FluentAssertions;
using FluentAssertions.Common;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;
using Xunit.Abstractions;
namespace TestProject1;

public class UnitTest1
{

    private readonly XUnitTextWriter output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = new XUnitTextWriter(output);
    }
    
    [Fact]
    public void Evaluate()
    {
        
        this.output.WriteLine("Result: " + this.Build($$"""
            {
                print("Hello" + "World" + "!"); 
                let f = 50;
                print(string(f));
            }
            """));
    }

    private object Build(string input)
    {
        var syntaxTree = SyntaxTree.Parse(input);
        var compilation = new Compilation(syntaxTree);
        
        this.output.WriteLine("Syntax Tree:");
        compilation.SyntaxTree.Root.WriteTo(this.output);
        this.output.WriteLine();
        this.output.WriteLine();
        this.output.WriteLine("Bound Tree:");
        compilation.EmitTree(this.output);
        
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
                this.output.WriteLine(line);
                this.output.WriteLine(diagnostic.Message);
            }
        }
        else
        {
            return evaluation.Result;
        }

        return evaluation.Diagnostics;
    }
}