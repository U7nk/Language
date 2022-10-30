using System;

namespace Language.Analysis.CodeAnalysis.Text;

public sealed class TextLocation : IEquatable<TextLocation>
{
    public TextLocation(SourceText text, TextSpan span)
    {
        Text = text;
        Span = span;
    }
    
    public SourceText Text { get; }
    public TextSpan Span { get; }

    public int StartLine => Text.GetLineIndex(Span.Start);
    public int EndLine => Text.GetLineIndex(Span.End);
    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
    public int EndCharacter => Span.End - Text.Lines[StartLine].Start;
    
    #region Equality

    public bool Equals(TextLocation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Text.Equals(other.Text) && Span.Equals(other.Span);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is TextLocation other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text, Span);
    }

    public static bool operator ==(TextLocation? left, TextLocation? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TextLocation? left, TextLocation? right)
    {
        return !Equals(left, right);
    }

    #endregion

}