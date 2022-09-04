using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxTree
{
    public SourceText SourceText { get; }
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public SyntaxTree(SourceText sourceText,
        ImmutableArray<Diagnostic> diagnostics,
        ExpressionSyntax root,
        SyntaxToken endOfFileToken)
    {
        this.SourceText = sourceText;
        this.Root = root;
        this.EndOfFileToken = endOfFileToken;
        this.Diagnostics = diagnostics;
    }
    
    public static SyntaxTree Parse(string source)
    {
        var sourceText = SourceText.ParseFrom(source);
        return Parse(sourceText);
    }
    public static SyntaxTree Parse(SourceText source)
    {
        var parser = new Parser(source);
        var syntaxTree = parser.Parse();
        return syntaxTree;
    }
    
    public static ICollection<SyntaxToken> ParseTokens(string source) 
        => ParseTokens(SourceText.ParseFrom(source));

    public static ICollection<SyntaxToken> ParseTokens(SourceText source)
    {
      var lexer = new Lexer(source);
      var tokens = lexer.Parse();
      return tokens;
    }
}