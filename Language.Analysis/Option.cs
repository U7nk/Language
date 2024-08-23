using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Language.Analysis.Extensions;

namespace Language.Analysis;

public struct Unit
{
    public static readonly Unit Default = default;
}
public static class Option
{
    public static Option<TY> Fold<TY>(this Option<Option<TY>> opt)
    {
        return opt.Unwrap();
    }
    public static Option<TY> Fold<TY>(this Option<Option<Option<TY>>> opt)
    {
        return opt.Unwrap().Unwrap();
    }
    
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
public readonly struct Option<T> : IEquatable<Option<T>>
{
    static Option<T> None { get; } = new(default!, hasValue: false);

    public Option(T value, bool hasValue = true)
    {
        Value = value;
        HasValue = hasValue;
    }
    
    bool HasValue { get; }

    public T SomeOr(T defaultValue)
    {
        return Value ?? defaultValue;
    }
    
    public T SomeOr(Func<T> defaultValue)
    {
        return Value ?? defaultValue();
    }
    
    [StackTraceHidden]
    public T Unwrap()
    {
        if (HasValue is false && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Option<>))
        {
            return default;
        }
        
        if (HasValue is false || Value is null)
            throw new InvalidOperationException("Option is empty");
        
        return Value;
    }
    
    public T? UnwrapOrNull()
    {
        return Value;
    }
    
    [StackTraceHidden]
    public TAs UnwrapAs<TAs>() where TAs : T 
    {
        return Value is TAs value 
            ? value 
            : throw new InvalidOperationException("Option is empty");
    }
    
    public bool IsNone => !IsSome;
    public bool IsSome
    {
        get
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Option<>))
            {
                var unwrap = this.Unwrap();
                if (unwrap is null)
                    throw new Exception();
                var prop = unwrap.GetType().GetProperty(nameof(IsSome));
                if (prop is null)
                    throw new Exception();
                var method = prop.GetMethod;
                if (method is null)
                    throw new Exception();
                
                return method.Invoke(this.Unwrap(), []).As<bool>();
            }
            
            return HasValue;
        }
    }

    T? Value { get; }
    
    public static implicit operator Option<T>(T? value)
    {
        
        
        if (value is null)
            return None;
        
        return Option.Some(value);
    }
    
    public static implicit operator Option<T>(Option<Unit> value) => None;
    public static implicit operator Option<T>(Option<Option<T>> value) => value.IsNone ? Option.None : value.Unwrap();
    
    public bool Equals(Option<T> other) 
        => HasValue == other.HasValue 
           && EqualityComparer<T?>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj)
    {
        return obj is Option<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HasValue, Value);
    }

    public void OnSome(Action<T> action)
    {
        if (HasValue)
            action(Value!);
    }
    
    public Option<TY> OnSome<TY>(Func<T, TY> action)
    {
        if (HasValue)
        {
            var val = action(Value!);
            return val;
        }
        return Option.None;
    }
    
    public IEnumerable<Y> SomeOrEmpty<Y>()
    {
        if (!typeof(T).CanBeConvertedTo<IEnumerable<Y>>())
        {
            throw new Exception("method can be used only with IEnumerable<Y>");
        }
        
        if (this.IsSome)
        {
            return (IEnumerable<Y>)this.Unwrap();
        }
        
        return (IEnumerable<Y>)typeof(Enumerable).GetMethod("Empty").NullGuard()
            .GetGenericMethodDefinition().MakeGenericMethod(typeof(Y))
            .Invoke(null, null)
            .NullGuard();
    }
}