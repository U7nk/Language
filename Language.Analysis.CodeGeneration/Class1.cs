﻿using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace CodeGen;

internal static class Extensions
{
    public static bool InheritsFrom(this INamedTypeSymbol? type, INamedTypeSymbol fromSymbol)
    {
        if (type == null)
            return false;

        if (SymbolEqualityComparer.Default.Equals(type, fromSymbol))
        {
            return true;
        }

        return InheritsFrom(type.BaseType, fromSymbol);
    }
}

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // if (!Debugger.IsAttached)
        // {
        //     Debugger.Launch();
        // }
        
        var syntaxTreeType = context.Compilation.GetTypeByMetadataName("Language.Analysis.CodeAnalysis.Syntax.SyntaxTree");
        var syntaxNodeType = context.Compilation.GetTypeByMetadataName("Language.Analysis.CodeAnalysis.Syntax.SyntaxNode");
        if (syntaxNodeType is not { })
        {
            throw new Exception("SyntaxNode not found");
        }
        if (syntaxTreeType is not { })
        {
            throw new Exception("SyntaxTree not found");
        }
        
        var syntaxNodeChildren = context.Compilation.Assembly.TypeNames
            .SelectMany(x => context.Compilation.GetSymbolsWithName(s=> s.Contains(x)))
            .Where(x=> x != null)
            .Where(x => x.Kind == SymbolKind.NamedType)
            .Cast<INamedTypeSymbol>()
            .Where(x => x.InheritsFrom(syntaxNodeType))
            .Where(x => !x.IsAbstract)
            .Distinct(SymbolEqualityComparer.Default)
            .Cast<INamedTypeSymbol>()
            .ToList();
        
        var methods = new List<string>();
        foreach (var child in syntaxNodeChildren)
        {
            var ctor = child.Constructors
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, syntaxTreeType));
            if (ctor == null)
                continue;

            var parameters = ctor.Parameters
                .Where(x=> !SymbolEqualityComparer.Default.Equals(x.Type, syntaxTreeType))
                .ToList();
            var parametersWithoutSyntaxTreeSource = string.Join(
                ",",
                parameters.Select(x => $"{x.Type.ToDisplayString()} {x.Name}").ToList());

            var argumentsSource = string.Join(
                ",",
                ctor.Parameters
                    .Select(x => SymbolEqualityComparer.Default.Equals(x.Type, syntaxTreeType) ? "this" : x.Name)
                    .ToList());
            
            var method = $$$"""
                    public {{{child.ToDisplayString()}}} Bind{{{child.Name}}}({{{parametersWithoutSyntaxTreeSource}}})
                    {
                       return new {{{child.ToDisplayString()}}}({{{argumentsSource}}});
                    }
                """;
            methods.Add(method);
        }
        
        var methodsSource = string.Join("\n\n", methods);
        // Build up the source code
        string source = $$$"""
            // <auto-generated/>
            #nullable enable
            using System;
            namespace Language.Analysis.CodeAnalysis.Syntax;
            public partial class SyntaxTree {
            {{{methodsSource}}}
            }
            """;
        
        var typeName = syntaxTreeType.ToDisplayString();

        // Add the source code to the compilation
        context.AddSource($"{typeName}.g.cs", source);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required for this one
    }
}