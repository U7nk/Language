using Language.Analysis.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Syntax;
using Mono.Options;

namespace compiler;

static class PathExtensions
{
    internal static string ChangeExtension(this string source, string newExtension) 
        => Path.ChangeExtension(source, newExtension);
}

static class Program
{
    static IEnumerable<string?> GetFilePaths(IEnumerable<string> paths)
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
    
    public static int Main(string[] args)
    {
        var references = new List<string>();
        var outputPath = (string?)null;
        var moduleName = (string?)null;
        var sourcePaths = new List<string>();
        var helpRequested = false;
        var options = new OptionSet
        {
            "usage: compiler <source-files> [options]",
            { "r=", "the {path} to the reference", v => references.Add(v) },
            { "o=", "the output {path} of the assembly  to create", v => outputPath = v },
            { "m=", "the {name} of the module", v => moduleName = v },
            { "?|h|help", _ => helpRequested = true  },
            { "<>", v => sourcePaths.Add(v) },
        };

        options.Parse(args);

        if (helpRequested)
        {
            options.WriteOptionDescriptions(Console.Out);
            return 0;
        }
        
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;
        var paths = GetFilePaths(sourcePaths);
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
            return -1;

        var compilation = Compilation.Create(syntaxTrees.ToArray());
        if (outputPath is null) 
            outputPath = sourcePaths[0].ChangeExtension(".exe");

        ConsoleEx.WriteLine(outputPath);
        var emitDiagnostics = compilation.Emit(moduleName, references.ToArray(), outputPath);

        if (emitDiagnostics.Any())
        {
            foreach (var diagnostic in emitDiagnostics)
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

        return 0;
    }
}