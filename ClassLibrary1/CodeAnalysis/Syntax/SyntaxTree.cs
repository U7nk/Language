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

    private SyntaxTree(SourceText sourceText)
    {
        this.SourceText = sourceText;
        var parser = new Parser(sourceText);
        this.Root = parser.ParseCompilationUnit();
        var diagnostics = parser.Diagnostic.ToImmutableArray();
        this.Diagnostics = diagnostics;
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
    {
      var lexer = new Lexer(source);
      var tokens = lexer.Parse();
      return tokens;
    }
}