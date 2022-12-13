using System;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis;

public sealed class Diagnostic : IEquatable<Diagnostic>
{
    public string Code { get; }
    public TextLocation TextLocation { get; }
    public string Message { get; }

    public Diagnostic(TextLocation textLocation, string message, string code)
    {
        TextLocation = textLocation;
        Message = message;
        Code = code;
    }
    
    public override string ToString() => $"{TextLocation.StartLine} - {TextLocation.EndLine}, {TextLocation.StartCharacter}..{TextLocation.EndCharacter}: " + Message;

    #region Equality 
    
    public bool Equals(Diagnostic? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && TextLocation.Equals(other.TextLocation) && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is Diagnostic other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, TextLocation, Message);
    }

    public static bool operator ==(Diagnostic? left, Diagnostic? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Diagnostic? left, Diagnostic? right)
    {
        return !Equals(left, right);
    }
    #endregion
}