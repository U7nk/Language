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

        if (!Equals(from, TypeSymbol.BuiltIn.Void()) && Equals(to, TypeSymbol.BuiltIn.Object()))
            return Implicit;
        
        if (Equals(from, TypeSymbol.BuiltIn.Object()) && !Equals(to, TypeSymbol.BuiltIn.Void()))
            return Explicit; 
        
        if (Equals(from, TypeSymbol.BuiltIn.Bool()) || Equals(from, TypeSymbol.BuiltIn.Int()))
        {
            if (Equals(to, TypeSymbol.BuiltIn.String()))
                return Explicit;
        }

        if (Equals(from, TypeSymbol.BuiltIn.String()))
        {
            if (Equals(to, TypeSymbol.BuiltIn.Bool()) || Equals(to, TypeSymbol.BuiltIn.Int()))
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