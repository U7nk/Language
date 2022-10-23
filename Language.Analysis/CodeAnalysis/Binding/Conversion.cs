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

        if (!Equals(from, TypeSymbol.Void) && Equals(to, TypeSymbol.Any))
            return Implicit;
        
        if (Equals(from, TypeSymbol.Any) && !Equals(to, TypeSymbol.Void))
            return Explicit; 
        
        if (Equals(from, TypeSymbol.Bool) || Equals(from, TypeSymbol.Int))
        {
            if (Equals(to, TypeSymbol.String))
                return Explicit;
        }

        if (Equals(from, TypeSymbol.String))
        {
            if (Equals(to, TypeSymbol.Bool) || Equals(to, TypeSymbol.Int))
                return Explicit;
        }

        return None;
    }

    public static Conversion Explicit { get; } = new(exists: true, isIdentity: false, isImplicit: false);
    public static Conversion Implicit { get; } = new(exists: true, isIdentity: false, isImplicit: true);
    public static Conversion None { get; } = new(exists: false, isIdentity: false, isImplicit: false);
    public static Conversion Identity { get; } = new(exists: true, isIdentity: true, isImplicit: true);
}