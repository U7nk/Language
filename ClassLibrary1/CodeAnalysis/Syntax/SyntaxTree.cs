using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxTree
{
    
    delegate void ParseHandler(SyntaxTree tree, 
    out CompilationUnitSyntax root, 
    out ImmutableArray<Diagnostic> diagnostics);

    SyntaxTree(SourceText sourceText, ParseHandler parseHandler)
    {
        SourceText = sourceText;
        
        parseHandler(this, out var root, out var diagnostics);
        
        Diagnostics = diagnostics;
        Root = root;
    }
    public SourceText SourceText { get; }
    public CompilationUnitSyntax Root { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    static void Parse(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
    {
        var parser = new Parser(syntaxTree);
        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }
    
    public static SyntaxTree Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText);
    }
    
    public static SyntaxTree Parse(string source)
    {
        var sourceText = SourceText.From(source);
        return Parse(sourceText);
    }
    
    public static SyntaxTree Parse(SourceText source) => new SyntaxTree(source, Parse);

    public static ICollection<SyntaxToken> ParseTokens(string source) 
        => ParseTokens(SourceText.From(source));

    public static ICollection<SyntaxToken> ParseTokens(SourceText source) 
        => ParseTokens(source, out _);

    public static ICollection<SyntaxToken> ParseTokens(string source, out ImmutableArray<Diagnostic> diagnostics) 
        => ParseTokens(SourceText.From(source), out diagnostics);

    public static ICollection<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        ICollection<SyntaxToken>? tokens = null;
        void ParseTokensLocal(SyntaxTree syntaxTree,
            out CompilationUnitSyntax root,
            out ImmutableArray<Diagnostic> diags)
        {
            var lexer = new Lexer(syntaxTree);
            tokens = lexer.Lex();
            root = new CompilationUnitSyntax(syntaxTree, ImmutableArray<MemberSyntax>.Empty, lexer.NextToken());
            diags = lexer.Diagnostics.ToImmutableArray();
        }
        
        
        
        var tree = new SyntaxTree(source, ParseTokensLocal);
        Debug.Assert(tokens != null, nameof(tokens) + " != null");
        diagnostics = tree.Diagnostics;
        return tokens.ToImmutableArray();
    }
}