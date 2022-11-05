using System;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Interpretation;

public class ObjectInstance : RuntimeObject
{
    public TypeSymbol Type { get; }
    public object? LiteralValue { get; }

    public static ObjectInstance Object(TypeSymbol type, Dictionary<string, ObjectInstance?> fields) => new(type, fields);
    public static ObjectInstance Literal(TypeSymbol type, object value) => new(type, new Dictionary<string, ObjectInstance?>(), value);

    private ObjectInstance(TypeSymbol type, Dictionary<string, ObjectInstance?> fields, object? literalValue = null) 
        : base(fields)
    {
        if (fields.Any() && literalValue is { })
        {
            throw new ArgumentException("Cannot have both fields and literal value");
        }

        LiteralValue = literalValue;
        Type = type;
    }
    
    public override string ToString()
    {
        return $$"""
                {{Type.Name}}
                {
                    LiteralValue: {{LiteralValue}};
                    
                    Fields:
                        {{string.Join(Environment.NewLine, Fields.Select(f => $"        {f.Key}: {f.Value}"))}}
                }
                """;
            
    }
}