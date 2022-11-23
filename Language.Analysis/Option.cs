using System;
using System.Collections;
using System.Collections.Generic;

namespace Language.Analysis;

public struct Unit { }
public static class Option
{
    public static Option<T> Some<T>(T value) => new(value, hasValue: true);

    public static Option<Unit> None { get; } = new(default, hasValue: false);

    public static Option<T> NoneIfEmpty<T>(T value) where T : IEnumerable
    {
        if (value is ICollection collection)
            return collection.Count == 0 ? None : Some(value);

        var enumerator = value.GetEnumerator();
        return enumerator.MoveNext() ? Some(value) : None;
    }
}
public readonly struct Option<T>
{
    static Option<T> None { get; } = new(default!, hasValue: false);

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
    
    public T? UnwrapOrNull()
    {
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
    
    public static implicit operator Option<T>(T? value)
    {
        if (value is null)
            return None;
        
        return Option.Some(value);
    }
    
    public static implicit operator Option<T>(Option<Unit> value) => None;
}