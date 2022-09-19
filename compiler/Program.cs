// See https://aka.ms/new-console-template for more information

using compiler;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: compiler <file>"); 
    return;
}

if (args.Length > 1)
{
    Console.WriteLine("Too many arguments. only one file can be compiled at a time.");
    return;
}

var path = args.Single();

var syntaxTree = SyntaxTree.Load(path);

var compilation = new Compilation(syntaxTree);
var result = compilation.Evaluate(new Dictionary<VariableSymbol, object?>());

if (result.Diagnostics.Any())
{
    foreach (var diagnostic in result.Diagnostics)
    {
        var sourceText = syntaxTree.SourceText;
        var text = sourceText.ToString();
        var lineIndex = sourceText.GetLineIndex(diagnostic.TextLocation.Span.Start); 
        var lineNumber = lineIndex + 1; 
        var prefix = syntaxTree.SourceText.ToString().Substring(0, diagnostic.TextLocation.Span.Start);
        var error = text.Substring(diagnostic.TextLocation.Span.Start, diagnostic.TextLocation.Span.Length);
        var suffix = text.Substring(diagnostic.TextLocation.Span.End);
        
        var line = $"{diagnostic.TextLocation.Text.FileName}" +
                   $"({diagnostic.TextLocation.StartLine + 1},{diagnostic.TextLocation.StartCharacter + 1}," +
                   $"{diagnostic.TextLocation.EndLine + 1},{diagnostic.TextLocation.EndCharacter + 1}):{diagnostic.Message}\n \"{prefix}";
        ConsoleEx.Write(line);
        ConsoleEx.Write($"|>", ConsoleColor.Red);
        ConsoleEx.Write($"{error}");
        ConsoleEx.Write($"<|", ConsoleColor.Red);
        ConsoleEx.Write($"{suffix}\"");
        ConsoleEx.WriteLine();
        ConsoleEx.Write(diagnostic.Message, ConsoleColor.DarkCyan);
        ConsoleEx.WriteLine();
    }
}


