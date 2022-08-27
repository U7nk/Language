using System.Collections.Generic;
using System.Linq;

namespace Wired.CodeAnalysis;

public class SyntaxTree
{
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    public SyntaxTree(
        IEnumerable<string> diagnostics,
        ExpressionSyntax root,
        SyntaxToken endOfFileToken)
    {
        this.Root = root;
        this.EndOfFileToken = endOfFileToken;
        this.Diagnostics = diagnostics.ToArray();
    }
    
    public static SyntaxTree Parse(string source)
    {
        var parser = new Parser(source);
        var syntaxTree = parser.Parse();
        return syntaxTree;
    }
}