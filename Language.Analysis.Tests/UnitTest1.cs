using System.CodeDom.Compiler;
using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Xunit.Abstractions;

namespace Language.Analysis.Tests;

public class UnitTest1
{
    readonly XUnitTextWriter _output;

    public UnitTest1(ITestOutputHelper output)
    {
        this._output = new XUnitTextWriter(output);
    }

    // object Build(string input)
    // {
    //     var syntaxTree = SyntaxTree.Parse(input);
    //     var compilation = Compilation.CreateScript(null, syntaxTree);
    //     
    //     _output.WriteLine("Syntax Tree:");
    //     compilation.SyntaxTrees.Single().Root.WriteTo(_output);
    //     _output.WriteLine();
    //     _output.WriteLine();
    //     _output.WriteLine("Bound Tree:");
    //     var tw = new StringWriter();
    //     var output = new IndentedTextWriter(tw);
    //     compilation.EmitTree(output);
    //     tw.ToString().Split(Environment.NewLine)
    //         .ToList()
    //         .ForEach(x => _output.WriteLine(x));
    //     
    //     var variables = new Dictionary<VariableSymbol, ObjectInstance?>();
    //     var evaluation = compilation.Evaluate(variables);
    //     if (evaluation.Diagnostics.Any())
    //     {
    //         foreach (var diagnostic in evaluation.Diagnostics)
    //         {
    //             var text = syntaxTree.SourceText;
    //             var lineIndex = text.GetLineIndex(diagnostic.TextLocation.Span.Start); 
    //             var lineNumber = lineIndex + 1; 
    //             var prefix = input.Substring(0, diagnostic.TextLocation.Span.Start);
    //             var error = input.Substring(diagnostic.TextLocation.Span.Start, diagnostic.TextLocation.Span.Length);
    //             var suffix = input.Substring(diagnostic.TextLocation.Span.End);
    //             var line = $"({lineNumber},{diagnostic.TextLocation.Span.Start - text.Lines[lineIndex].Start + 1}): \"{prefix}> {error} <{suffix}\"";
    //             _output.WriteLine(line);
    //             _output.WriteLine(diagnostic.Message);
    //         }
    //     }
    //     else
    //     {
    //         return evaluation.Result ?? throw new Exception();
    //     }
    //
    //     return evaluation.Diagnostics;
    // }
}