using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wired.CodeAnalysis.Syntax;

namespace Wired;

internal static class Extensions
{
        
    public static RangeEnumerator GetEnumerator(this Range range) => new(range);

    public ref struct RangeEnumerator
    {
        int _current;
        readonly int _end;
        readonly sbyte _step;

        public RangeEnumerator(Range range)
        {
            if (range.Start.IsFromEnd)
            {
                throw new NotSupportedException("Start from end is not supported");
            }
            
            if (range.Start.Value > range.End.Value)
            {
                _current = range.Start.Value;
                _end = range.End.Value - 1;
                _step = -1;
            }
            else
            {
                _current = range.Start.Value - 1;
                _end = range.End.Value;
                _step = 1;
            }
        }
        
        public bool MoveNext()
        {
            _current += _step;
            return _current != _end;
        }
        
        public int Current => _current;
    }
    
    [DebuggerHidden]
    [DebuggerStepThrough]
    [return: NotNull]
    public static T Unwrap<T>(
        [NotNull]this T? obj,
        [CallerArgumentExpression("obj")] string objExpression = "")
    {
        if (obj is null)
        {
            throw new ArgumentNullException(objExpression);
        }

        return obj;
    }
    
    [DebuggerHidden]
    [DebuggerStepThrough]
    [return: NotNull]
    public static T Unwrap<T>(
        [NotNull]this object? obj,
        [CallerArgumentExpression("obj")] string objExpression = "")
    {
        if (obj is null)
        {
            throw new ArgumentNullException(objExpression);
        }

        return (T)obj;
    }


    internal static bool Empty<T>(this IEnumerable<T> enumerable) 
        => !enumerable.Any();

    internal static IEnumerable<T> Exclude<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        => enumerable.Where(x => !predicate(x));

    internal static string CutTail(this string str, int count)
    {
        return str.Substring(0, str.Length - count);
    }
        
    [DebuggerStepThrough]
    internal static T As<T>(this object toCast)
    {
        return (T)toCast;
    }

    [DebuggerStepThrough]
    internal static T AddTo<T>(this T obj, ICollection<T> collection)
    {
        collection.Add(obj);
        return obj;
    }
    
    [DebuggerStepThrough]
    internal static IEnumerable<T> AddRangeTo<T>(this ICollection<T> enumerable, ICollection<T> collection)
    {
        foreach (var obj in enumerable)
        {
            collection.Add(obj);
        }
        
        return enumerable;
    }
    
    [DebuggerStepThrough]
    internal static IEnumerable<T> Only<T>(this IEnumerable<SyntaxTree> syntaxTrees)
    {
        return syntaxTrees
            .SelectMany(st => st.Root.Members)
            .OfType<T>();
    }
    
    
    
    [DebuggerStepThrough]
    internal static void ForEach<T>(this ICollection<T> enumerable, Action<T> action)
    {
        foreach (var obj in enumerable)
        {
            action(obj);
        }
    }

    internal static T[] Slice<T>(this T[] collection, int fromIncluding, int toExcluding = -1)
    {
        if (toExcluding == -1)
        {
            toExcluding = collection.Length;
        }
        var res = new T[toExcluding - fromIncluding];
        var counter = 0;
        foreach(var i in 0..collection.Length)
        {
            if (i < fromIncluding || i >= toExcluding)
            {
                continue;
            }
            res[counter] = collection[i];
        }
        return res;
    }

    internal static IList<T> Slice<T>(this IList<T> collection, int fromIncluding, int toExcluding = -1)
    {
        if (toExcluding == -1)
        {
            toExcluding = collection.Count;
        }
        var res = new List<T>(toExcluding - fromIncluding);
        for (int i = 0; i < collection.Count; i++)
        {
            if (i < fromIncluding || i >= toExcluding)
            {
                continue;
            }
            res.Add(collection[i]);
        }
        return res;
    }
    

    public static bool CanBeConvertedTo<T>(this Type givenType, Type type) => 
        type.IsAssignableFrom(givenType);

    public static bool InRange(this int value, int left, int right)
    {
        if (left <= value && right >= value)
            return true;
        return false;
    } 
    public static bool CanBeConvertedTo<T>(this Type givenType) => 
        typeof(T).IsAssignableFrom(givenType);
    

    public static ConditionalAdd<T> If<T>(this T obj, bool condition)
    {
        return new ConditionalAdd<T>(obj);
    }
}
internal class ConditionalAdd<T>
{
    public bool IfState { get; set; }
    public bool Condition { get; set; }
    public List<Action> TrueActions { get; }
    public T Obj;
    public ConditionalAdd(T obj)
    {
        this.Obj = obj;
        TrueActions = new List<Action>();
    }
    public ConditionalAdd<T> If(bool condition)
    {
        Condition = condition;
        IfState = true;
        return this;
    }
    public ConditionalAdd<T> AddTo(IList list)
    {
        if (IfState)
        {
            if (Condition == true)
            {
                list.Add(Obj);
                return this;
            }
        }
        if (IfState == false)
        {
            if (Condition == false)
            {
                list.Add(Obj);
                return this;
            }
        }
        return this;
    }

    public ConditionalAdd<T> Else
    {
        get
        {
            IfState = false;
            return this;
        }
    }
}