

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;
using OneOf;
namespace Language.Analysis.CodeAnalysis.Syntax;

public class OneOfAttribute : Attribute
{
    public OneOfAttribute(Type type, string name, Type type2, string name2)
    {
        
    }
}

[OneOf(typeof(List<NamespaceSyntax>), "Namespaces", typeof(ImmutableArray<IGlobalMemberSyntax>), "GlobalStatements")]
public partial class NamespacesOrGlobalStatements { }




public sealed class CompilationUnitSyntax : SyntaxNode
{

    public CompilationUnitSyntax(SyntaxTree syntaxTree, NamespacesOrGlobalStatements namespacesOrGlobalStatements, SyntaxToken endOfFileToken) 
        : base(syntaxTree)
    {
        this.NamespacesOrGlobalStatements = namespacesOrGlobalStatements;
        EndOfFileToken = endOfFileToken;
    }

    public NamespacesOrGlobalStatements NamespacesOrGlobalStatements { get; }
    public SyntaxToken EndOfFileToken { get; }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}