using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

public class TypeSymbol : Symbol
{
    public static readonly TypeSymbol Error = new("error");
    public static readonly TypeSymbol Void = new("void");
    public static readonly TypeSymbol Bool = new("bool");
    public static readonly TypeSymbol Int = new("int");
    public static readonly TypeSymbol String = new("string");
    private TypeSymbol(string name) : base(name)
    {
    }

    public static TypeSymbol FromLiteral(SyntaxToken literalToken)
    {
        if (literalToken.Kind is SyntaxKind.TrueKeyword or SyntaxKind.FalseKeyword)
            return Bool;

        if (literalToken.Kind is SyntaxKind.NumberToken)
            return Int;
        
        if (literalToken.Kind is SyntaxKind.StringToken)
            return String;
        
        return Error;
    }

    public override SymbolKind Kind => SymbolKind.Type;
}