using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders.Field;

internal class FullFieldBinder
{
    readonly TypeSymbol _currentType;
    FieldSignatureBinder FieldSignatureBinder { get; }
    FieldSymbol FieldSymbol { get; set; }

    public FullFieldBinder(BoundScope fieldScope, TypeSymbol currentType, DeclarationsBag allDeclarations)
    {
        _currentType = currentType;
        FieldSignatureBinder = new FieldSignatureBinder(
            fieldScope, 
            _currentType, allDeclarations);
    }
    public void BindDeclaration(FieldDeclarationSyntax fieldDeclarationSyntax, DiagnosticBag diagnostics)
    {
        FieldSymbol = FieldSignatureBinder.BindDeclaration(fieldDeclarationSyntax, diagnostics);
    }
}