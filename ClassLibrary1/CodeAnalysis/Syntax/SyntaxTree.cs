using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxTree
{
    public SourceText SourceText { get; }
    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    SyntaxTree(SourceText sourceText)
    {
        SourceText = sourceText;
        var parser = new Parser(sourceText);
        Root = parser.ParseCompilationUnit();
        var diagnostics = parser.Diagnostic.ToImmutableArray();
        Diagnostics = diagnostics;
    }
    
    public static SyntaxTree Parse(string source)
    {
        var sourceText = SourceText.From(source);
        return Parse(sourceText);
    }
    public static SyntaxTree Parse(SourceText source)
    {
        
        return new SyntaxTree(source);
    }
    
    public static ICollection<SyntaxToken> ParseTokens(string source) 
        => ParseTokens(SourceText.From(source));

    public static ICollection<SyntaxToken> ParseTokens(SourceText source) 
        => ParseTokens(source, out _);

    public static ICollection<SyntaxToken> ParseTokens(string source, out ImmutableArray<Diagnostic> diagnostics) 
        => ParseTokens(SourceText.From(source), out diagnostics);

    public static ICollection<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Parse();
        diagnostics = lexer.Diagnostics.ToImmutableArray();
        return tokens;
    }
}