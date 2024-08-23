using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Field;

public sealed class FieldSignatureBinder
{
    readonly BoundScope _scope;
    readonly TypeSymbol _containingType;
    readonly DeclarationsBag _allDeclarations;

    public FieldSignatureBinder(BoundScope scope, TypeSymbol containingType, DeclarationsBag allDeclarations)
    {
        _scope = scope;
        _containingType = containingType;
        _allDeclarations = allDeclarations;
    }

    public FieldSymbol BindDeclaration(FieldDeclarationSyntax fieldDeclaration, DiagnosticBag diagnostics)
    {
        var fieldTypeOrNone = _scope.TryLookupType(fieldDeclaration.TypeClause.NamedTypeExpression.GetName(), _containingType.ContainingNamespace); 
        if (fieldTypeOrNone.IsNone)
        {
            diagnostics.ReportUndefinedType(
                fieldDeclaration.TypeClause.Location,
                fieldDeclaration.TypeClause.NamedTypeExpression.GetName());
        }

        // if diagnostics are reported field should not be used later in binding
        // so we just let type to be null and try to gain more diagnostics
        var fieldSymbol = new FieldSymbol(fieldDeclaration,
                                    fieldDeclaration.StaticKeyword.IsSome,
                                    fieldDeclaration.Identifier.Text,
                                    _containingType, 
                                    fieldTypeOrNone.SomeOr(TypeSymbol.BuiltIn.Error));
        _allDeclarations.AddDeclaration(fieldSymbol, fieldDeclaration, diagnostics);

        return fieldSymbol;
    }
}