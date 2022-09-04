using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxTree
{
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public SyntaxTree(
        ImmutableArray<Diagnostic> diagnostics,
        ExpressionSyntax root,
        SyntaxToken endOfFileToken)
    {
        this.Root = root;
        this.EndOfFileToken = endOfFileToken;
        this.Diagnostics = diagnostics;
    }
    
    public static SyntaxTree Parse(string source)
    {
        var parser = new Parser(source);
        var syntaxTree = parser.Parse();
        return syntaxTree;
    }
    
    public static ICollection<SyntaxToken> ParseTokens(string source)
    {
      var lexer = new Lexer(source);
      var tokens = lexer.Parse();
      return tokens;
    }
}