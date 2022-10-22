using System;

namespace Language.Analysis;

/// <summary>
/// Not thread safe
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingleInitialized<T>
{
    public T Value { get; private set; } = default!;
    public bool IsInitialized { get; private set; }
    public SingleInitialized(T value)
    {
        Value = value;
        IsInitialized = true;
    }

    public SingleInitialized()
    {
        IsInitialized = false;
    }
    
    public void Set(T value)
    {
        if (IsInitialized)
            throw new InvalidOperationException("Already initialized");
        
        Value = value;
        IsInitialized = true;
    }
    
    public static implicit operator T(SingleInitialized<T> singleInitialized)
    {
        if (!singleInitialized.IsInitialized)
            throw new InvalidOperationException("Not initialized");
        
        return singleInitialized.Value;
    }
}