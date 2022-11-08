using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeSignatureBinder
{
    readonly BoundScope _scope;

    public TypeSignatureBinder(BoundScope scope)
    {
        _scope = scope;
    }

    public (ImmutableArray<Diagnostic> Diagnostics, TypeSymbol TypeSymbol) BindClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.Text;
        var type = TypeSymbol.New(name, ImmutableArray.Create<SyntaxNode>(classDeclaration), 
                                  classDeclaration.InheritanceClause, new MethodTable(), new FieldTable());
        if (_scope.TryDeclareType(type))
            return (ImmutableArray<Diagnostic>.Empty, type);

        var diagnostics = new DiagnosticBag();
        
        _scope.TryLookupType(name, out var existingType).ThrowIfFalse();
        existingType.NG().AddDeclaration(classDeclaration);
        
        foreach (var syntaxNode in existingType.DeclarationSyntax.Cast<ClassDeclarationSyntax>())
        {
            diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                syntaxNode.Identifier.Location,
                syntaxNode.Identifier.Text);
        }
        
        return (diagnostics.ToImmutableArray(), type);
    }
}