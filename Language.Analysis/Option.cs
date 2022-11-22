using System;

namespace Language.Analysis;

public readonly struct Option<T>
{
    public static Option<T> None { get; } = new(default!, hasValue: false);
    public static Option<T> Some(T value) => new(value, hasValue: true);
    
    public Option(T value, bool hasValue = true)
    {
        Value = value;
        HasValue = hasValue;
    }

    bool HasValue { get; }

    public T Unwrap()
    {
        if (Value is null)
            throw new InvalidOperationException("Option is empty");
        
        return Value;
    }

    public TAs UnwrapAs<TAs>() where TAs : T 
    {
        return Value is TAs value 
            ? value 
            : throw new InvalidOperationException("Option is empty");
    }
    
    public bool IsNone => !HasValue;
    public bool IsSome => HasValue;

    T? Value { get; }
}