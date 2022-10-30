using System;

namespace Language.Analysis.CodeAnalysis.Text;

public readonly struct TextSpan : IEquatable<TextSpan>
{
    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }
    public static TextSpan FromBounds(int firstStart, int lastEnd) => new(firstStart, lastEnd - firstStart);
    
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;

    public override string ToString() => $"{Start}..{End}";

    #region Equality
    
    public bool Equals(TextSpan other)
    {
        return Start == other.Start && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is TextSpan other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }

    public static bool operator ==(TextSpan left, TextSpan right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TextSpan left, TextSpan right)
    {
        return !left.Equals(right);
    }
    
    #endregion
}