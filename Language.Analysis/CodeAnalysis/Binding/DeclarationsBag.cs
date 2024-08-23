using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding;


public class DeclarationsBag : Dictionary<Symbol, List<SyntaxNode>>
{
    class DeclarationEqualityComparer : IEqualityComparer<Symbol>
    {
        public bool Equals(Symbol? left, Symbol? right)
        {
            left.NullGuard("left symbol cannot be null");
            right.NullGuard("right symbol cannot be null");
            
            return left.DeclarationEquals(right);
        }

        public int GetHashCode(Symbol obj)
        {
            return obj.DeclarationHashCode();
        }
    }
    public DeclarationsBag() : base(new DeclarationEqualityComparer())
    {
    }
    
    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. <br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<SyntaxNode> LookupDeclarations(Symbol symbol) =>
        this.TryGetValue(symbol, out var declarations) 
            ? declarations.ToImmutableArray() 
            : ImmutableArray<SyntaxNode>.Empty;

    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. And casts them to T.<br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<T> LookupDeclarations<T>(Symbol symbol) where T : SyntaxNode =>
        this.TryGetValue(symbol, out var declarations) 
            ? declarations.Cast<T>().ToImmutableArray() 
            : ImmutableArray<T>.Empty;

    public void AddDeclaration(Symbol boundNode, SyntaxNode declaration)
    {
        if (!this.TryGetValue(boundNode, out var declarations))
        {
            declarations = new List<SyntaxNode>();
            this.Add(boundNode, declarations);
        }
        
        declarations.Add(declaration);
    }
    
    public void AddDeclaration(FieldSymbol fieldSymbol, FieldDeclarationSyntax fieldDeclaration, DiagnosticBag diagnostics)
    {
        if (!this.TryGetValue(fieldSymbol, out var declarations))
        {
            declarations = new List<SyntaxNode>();
            this.Add(fieldSymbol, declarations);
        }
        
        declarations.Add(fieldDeclaration);
        
        var declareFieldRes = fieldSymbol.ContainingType.Unwrap().TryDeclareField(fieldSymbol);

        foreach (var res in declareFieldRes)
        {
            switch (res)
            {
                case TypeSymbol.TryDeclareFieldResult.TypeNameSameAsFieldName:
                    diagnostics.ReportClassMemberCannotHaveNameOfClass(fieldDeclaration.Identifier);
                    break;
                case TypeSymbol.TryDeclareFieldResult.FieldWithSameNameExists:
                {
                    var existingFieldDeclarations = this.LookupDeclarations<FieldDeclarationSyntax>(fieldSymbol);
                    if (existingFieldDeclarations.Any())
                    {
                        foreach (var existingFieldDeclaration in existingFieldDeclarations)
                        {
                            diagnostics.ReportFieldAlreadyDeclared(existingFieldDeclaration.Identifier);
                        }
                    }

                    break;
                }
                case TypeSymbol.TryDeclareFieldResult.MethodWithSameNameExists:
                {
                    var sameNameMethods = fieldSymbol.ContainingType.Unwrap().MethodTable.Where(x => x.MethodSymbol.Name == fieldSymbol.Name).ToList();
                    if (sameNameMethods.Any())
                    {
                        diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
                        foreach (var declaration in sameNameMethods)
                        {
                            var sameNameMethodDeclarations = this.LookupDeclarations<MethodDeclarationSyntax>(declaration.MethodSymbol);
                            foreach (var sameNameMethodDeclaration in sameNameMethodDeclarations)
                            {
                                diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(sameNameMethodDeclaration.Identifier);    
                            }
                        }
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }   
        }
    }
}