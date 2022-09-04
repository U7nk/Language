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
    public void Custom()
    {
        this.output.WriteLine("Result: " + this.Build("(a = ) == (a)"));
    }

    private object Build(string input)
    {
        var syntaxTree = SyntaxTree.Parse(input);
        syntaxTree.Root.WriteTo(this.output);
        var variables = new Dictionary<VariableSymbol, object?>();
        var binder = new Binder(variables);
        var bindTree = binder.BindExpression(syntaxTree.Root);
        var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToList(); 
        if (diagnostics.Any())
        {
            foreach (var diagnostic in diagnostics)
            {
                var text = syntaxTree.SourceText;
                var lineIndex = text.GetLineIndex(diagnostic.Span.Start); 
                var lineNumber = lineIndex + 1; 
                var prefix = input.Substring(0, diagnostic.Span.Start);
                var error = input.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                var suffix = input.Substring(diagnostic.Span.End);
                var line = $"({lineNumber},{diagnostic.Span.Start - text.Lines[lineIndex].Start + 1}): \"{prefix}~{error}~{suffix}\"";
                this.output.WriteLine(line);
                this.output.WriteLine(diagnostic.Message);
            }
        }
        else
        {
            var e = new Evaluator(bindTree, variables);
            var result = e.Evaluate();
            return result;
        }

        return diagnostics;
    }
}