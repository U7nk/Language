using System.Collections.Immutable;
using Language.CodeAnalysis.Symbols;
using Language.CodeAnalysis.Syntax;

namespace Language.CodeAnalysis.Binding.Binders;

sealed class TypeSignatureBinder
{
    readonly BoundScope _scope;

    public TypeSignatureBinder(BoundScope scope)
    {
        _scope = scope;
    }

    public ImmutableArray<Diagnostic> BindClassDeclaration(ClassDeclarationSyntax classDeclaration)
    {
        var name = classDeclaration.Identifier.Text;
        var type = TypeSymbol.New(name, classDeclaration, new MethodTable(), new FieldTable());
        if (_scope.TryDeclareType(type))
        {
            return ImmutableArray<Diagnostic>.Empty;
        }

        var diagnostics = new DiagnosticBag();
        diagnostics.ReportClassWithThatNameIsAlreadyDeclared(
            classDeclaration.Identifier.Location,
            classDeclaration.Identifier.Text);
        
        return diagnostics.ToImmutableArray();
    }
}