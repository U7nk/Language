using compiler;
using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

IEnumerable<string?> GetFilePaths(IEnumerable<string> paths)
{
    var result = new SortedSet<string>();
    foreach (var path in paths)
    {
        if (Directory.Exists(path))
        {
            result.UnionWith(Directory.EnumerateFiles(path, "*.ln", SearchOption.AllDirectories));
        }
        else if (File.Exists(path))
        {
            if (Path.GetExtension(path) == ".ln")
                result.Add(path);
        }
    }
    
    return result;
}

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: compiler <file1> <file2> ... <fileN>"); 
    return;
}

var paths = GetFilePaths(args);

var syntaxTrees = new List<SyntaxTree>();
var hasErrors = false;
foreach (var path in paths)
{
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"error: file '{path}' does not exist.");
        hasErrors = true;
        continue;
    }

    var syntaxTree = SyntaxTree.Load(path);
    syntaxTrees.Add(syntaxTree);
}

if (hasErrors)
    return;

var compilation = Compilation.Create(syntaxTrees.ToArray());
var result = compilation.Evaluate(new Dictionary<VariableSymbol, object?>());

if (result.Diagnostics.Any())
{
    foreach (var diagnostic in result.Diagnostics)
    {
        var sourceText = diagnostic.TextLocation.Text;
        var text = sourceText.ToString();
        var prefix = sourceText.ToString().Substring(0, diagnostic.TextLocation.Span.Start);
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
