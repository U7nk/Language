using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

public sealed class FieldSignatureBinder
{
    readonly FieldBinderLookup? _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public FieldSignatureBinder(BoundScope scope, bool isScript, FieldBinderLookup? lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public void BindDeclaration(FieldDeclarationSyntax fieldDeclaration, DiagnosticBag diagnostics)
    {
        if (!_scope.TryLookupType(fieldDeclaration.TypeClause.Identifier.Text, out var fieldType))
        {
            diagnostics.ReportUndefinedType(
                fieldDeclaration.TypeClause.Location,
                fieldDeclaration.TypeClause.Identifier.Text);
        }

        // if diagnostics are reported field should not be used later in binding
        // so we just let type to be null and try to gain more diagnostics
        var fieldSymbol = new FieldSymbol(fieldDeclaration,
                                    fieldDeclaration.StaticKeyword.IsSome,
                                    fieldDeclaration.Identifier.Text,
                                    _lookup.ContainingType, 
                                    fieldType!);
        _lookup.AddDeclaration(fieldSymbol, fieldDeclaration);
        if (!_lookup.ContainingType.TryDeclareField(fieldSymbol))
        {
            if (fieldSymbol.Name == _lookup.ContainingType.Name)
                diagnostics.ReportClassMemberCannotHaveNameOfClass(fieldDeclaration.Identifier);

            var existingFieldDeclarations = _lookup.LookupDeclarations<FieldDeclarationSyntax>(fieldSymbol);
            if (existingFieldDeclarations.Length > 1)
            {
                foreach (var existingFieldDeclaration in existingFieldDeclarations)
                {
                    diagnostics.ReportFieldAlreadyDeclared(existingFieldDeclaration.Identifier);
                }
            }

            
            var sameNameMethods = _lookup.ContainingType.MethodTable.Where(x => x.MethodSymbol.Name == fieldSymbol.Name).ToList();
            if (sameNameMethods.Any())
            {
                diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
                foreach (var declaration in sameNameMethods)
                {
                    var sameNameMethodDeclarations = _lookup.LookupDeclarations<MethodDeclarationSyntax>(declaration.MethodSymbol);
                    foreach (var sameNameMethodDeclaration in sameNameMethodDeclarations)
                    {
                        diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(sameNameMethodDeclaration.Identifier);    
                    }
                }
            }
        }
    }
}