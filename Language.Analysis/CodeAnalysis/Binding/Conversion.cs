using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class Conversion
{
    public bool Exists { get; }
    public bool IsIdentity { get; }
    public bool IsImplicit { get; }

    public Conversion(bool exists, bool isIdentity, bool isImplicit)
    {
        Exists = exists;
        IsIdentity = isIdentity;
        IsImplicit = isImplicit;
    }

    public static Conversion Classify(TypeSymbol from, TypeSymbol to)
    {
        if (Equals(from, to))
            return Identity;

        if (!Equals(from, BuiltInTypeSymbols.Void) && Equals(to, BuiltInTypeSymbols.Object))
            return Implicit;
        
        if (Equals(from, BuiltInTypeSymbols.Object) && !Equals(to, BuiltInTypeSymbols.Void))
            return Explicit; 
        
        if (Equals(from, BuiltInTypeSymbols.Bool) || Equals(from, BuiltInTypeSymbols.Int))
        {
            if (Equals(to, BuiltInTypeSymbols.String))
                return Explicit;
        }

        if (Equals(from, BuiltInTypeSymbols.String))
        {
            if (Equals(to, BuiltInTypeSymbols.Bool) || Equals(to, BuiltInTypeSymbols.Int))
                return Explicit;
        }

        if (from.IsSubClassOf(to))
            return Implicit;

        return None;
    }

    public static Conversion Explicit { get; } = new(exists: true, isIdentity: false, isImplicit: false);
    public static Conversion Implicit { get; } = new(exists: true, isIdentity: false, isImplicit: true);
    public static Conversion None { get; } = new(exists: false, isIdentity: false, isImplicit: false);
    public static Conversion Identity { get; } = new(exists: true, isIdentity: true, isImplicit: true);
}