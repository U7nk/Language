using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class MethodSymbol : MemberSymbol
{
    public MethodSymbol(Option<SyntaxNode> declaration, TypeSymbol? containingType, bool isStatic,
                        string name, ImmutableArray<ParameterSymbol> parameters,
                        TypeSymbol returnType)
        : base(declaration, name, containingType, returnType)
    {
        Parameters = parameters;
        IsStatic = isStatic;
    }

    public bool IsGeneratedFromGlobalStatements => DeclarationSyntax.IsSome &&
                                                   DeclarationSyntax.Unwrap() is
                                                       CompilerGeneratedGlobalStatementsDeclarationsBlockStatementSyntax;

    public bool IsStatic { get; }
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => Type;
    public override SymbolKind Kind => SymbolKind.Method;

    public override bool DeclarationEquals(Symbol other)
    {
        if (Kind != other.Kind)
            return false;

        if (Name != other.Name)
            return false;

        if (ContainingType != other.ContainingType)
            return false;

        return true;
    }

    public override int DeclarationHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Kind);
        hashCode.Add(Name);
        hashCode.Add(ContainingType);
        return hashCode.ToHashCode();
    }
}