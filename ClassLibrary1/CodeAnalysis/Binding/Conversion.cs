namespace Wired.CodeAnalysis.Binding;

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
        if (from == to)
            return Identity;

        if (from == TypeSymbol.Bool ||
            from == TypeSymbol.Int)
        {
            if (to == TypeSymbol.String)
                return Explicit;
        }

        if (from == TypeSymbol.String)
        {
            if (to == TypeSymbol.Bool || to == TypeSymbol.Int)
                return Explicit;
        }

        return None;
    }

    public static Conversion Explicit { get; } = new(exists: true, isIdentity: false, isImplicit: false);
    public static Conversion Implicit { get; } = new(exists: true, isIdentity: false, isImplicit: true);
    public static Conversion None { get; } = new(exists: false, isIdentity: false, isImplicit: false);
    public static Conversion Identity { get; } = new(exists: true, isIdentity: true, isImplicit: true);
}