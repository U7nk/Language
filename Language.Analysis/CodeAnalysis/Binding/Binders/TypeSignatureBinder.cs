using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeSignatureBinder
{
    readonly BoundScope _scope;
    readonly BinderLookup _lookup;

    public TypeSignatureBinder(BoundScope scope, BinderLookup lookup)
    {
        _scope = scope;
        _lookup = lookup;
    }

    public ImmutableArray<Diagnostic> BindClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.Text;
        var typeSymbol = TypeSymbol.New(name,
                                        classDeclaration, 
                                        classDeclaration.InheritanceClause,
                                        new MethodTable(),
                                        new FieldTable());
        _lookup.AddDeclaration(typeSymbol, classDeclaration);
        
        var diagnostics = new DiagnosticBag();
        if (!_scope.TryDeclareType(typeSymbol))
        {
            var existingClassDeclarationSyntax = _lookup.LookupDeclarations<ClassDeclarationSyntax>(typeSymbol);
            foreach (var syntaxNode in existingClassDeclarationSyntax)
            {
                diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
                    syntaxNode.Identifier.Location,
                    syntaxNode.Identifier.Text);
            }

            return diagnostics.ToImmutableArray();
        }

        return diagnostics.ToImmutableArray();
    }
}