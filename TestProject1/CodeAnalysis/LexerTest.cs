using Wired.CodeAnalysis.Syntax;

namespace TestProject1.CodeAnalysis;

public sealed class LexerTest
{
  [Theory]
  [MemberData(nameof(GetTokensData))]
  public void Lexer_Lexes_Token(SyntaxKind kind, string source)
  {
    var tokens = SyntaxTree.Parse(source);
  }

  public static IEnumerable<object[]> GetTokensData()
  {
    foreach (var token in GetTokens()) {
      yield return new object[] {token.kind, token.source};
    }
  }
   private static IEnumerable<(SyntaxKind kind, string source)> GetTokens()
  {
    return new[] {
      (SyntaxKind.IdentifierToken, "a"),
      (SyntaxKind.IdentifierToken, "abc"),
    };
  }
}