using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

internal class FullFieldBinder
{
    readonly bool _isScript;
    readonly TypeSymbol _currentType;
    FieldSignatureBinder FieldSignatureBinder { get; }
    FieldSymbol FieldSymbol { get; set; }

    public FullFieldBinder(BoundScope fieldScope, bool isScript, TypeSymbol currentType, DeclarationsBag allDeclarations)
    {
        _isScript = isScript;
        _currentType = currentType;
        FieldSignatureBinder = new FieldSignatureBinder(
            fieldScope, 
            _isScript,
            _currentType, allDeclarations);
    }
    public void BindDeclaration(FieldDeclarationSyntax fieldDeclarationSyntax, DiagnosticBag diagnostics)
    {
        FieldSymbol = FieldSignatureBinder.BindDeclaration(fieldDeclarationSyntax, diagnostics);
    }
}