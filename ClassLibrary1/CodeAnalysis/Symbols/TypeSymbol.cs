using System.Collections.Generic;
using System.Collections.Immutable;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Symbols;

public class TypeSymbol : Symbol
{
    public static readonly TypeSymbol Error = new("error", null, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Any = new("any", null, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Void = new("void", null, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Bool = new("bool", null, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol Int = new("int", null, new MethodTable(), new FieldTable());
    public static readonly TypeSymbol String = new("string", null, new MethodTable(), new FieldTable());
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
    public static TypeSymbol New(string name, ClassDeclarationSyntax? declaration,
        MethodTable methodTable, FieldTable fieldTable) 
        => new(name, declaration, methodTable, fieldTable);

    TypeSymbol(string name, ClassDeclarationSyntax? declaration, MethodTable methodTable, FieldTable fieldTable) : base(name)
    {
        Declaration = declaration;
        MethodTable = methodTable;
        FieldTable = fieldTable;
    }

    public ClassDeclarationSyntax? Declaration { get; }
    public MethodTable MethodTable { get; }
    public FieldTable FieldTable { get; }

    public override SymbolKind Kind => SymbolKind.Type;
}