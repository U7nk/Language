using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class MethodSymbol : MemberSymbol
{
    public MethodSymbol(Option<SyntaxNode> declaration, 
                        TypeSymbol? containingType, 
                        bool isStatic, 
                        bool isVirtual,
                        bool isOverriding,
                        string name, 
                        ImmutableArray<ParameterSymbol> parameters,
                        TypeSymbol returnType, bool isGeneric, Option<ImmutableArray<TypeSymbol>> genericParameters) 
        : base(declaration, name, containingType, returnType)
    {
        Parameters = parameters;
        IsGeneric = isGeneric;
        GenericParameters = genericParameters;
        IsVirtual = isVirtual;
        IsOverriding = isOverriding;
        IsStatic = isStatic;

        (!genericParameters.IsSome && isGeneric || genericParameters.IsSome && !isGeneric)
            .EnsureFalse("Method cannot be generic and not define generic parameters.");
    }
    
    public bool IsGeneratedFromGlobalStatements => DeclarationSyntax.IsSome &&
                                                   DeclarationSyntax.Unwrap() is CompilerGeneratedBlockOfGlobalStatementsSyntax;

    public bool IsStatic { get; }
    public bool IsVirtual { get; }
    public bool IsOverriding { get; }

    public bool IsGeneric { get; } 
    public Option<ImmutableArray<TypeSymbol>> GenericParameters { get; }
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => Type;
    public override SymbolKind Kind => SymbolKind.Method;

    public override bool DeclarationEquals(Symbol other)
    {
        if (Kind != other.Kind)
            return false;

        if (Name != other.Name)
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