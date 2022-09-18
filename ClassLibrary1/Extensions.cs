using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Wired;

internal static class Extensions
{
    [DebuggerHidden]
    [DebuggerStepThrough]
    [return: NotNull]
    public static T ThrowIfNull<T>(
        [NotNull]this T? obj,
        [CallerArgumentExpression("obj")] string objExpression = "")
    {
        if (obj is null)
        {
            throw new ArgumentNullException(objExpression);
        }

        return obj;
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

    internal static T[] Slice<T>(this T[] collection, int fromIncluding, int toExcluding = -1)
    {
        if (toExcluding == -1)
        {
            toExcluding = collection.Length;
        }
        var res = new T[toExcluding - fromIncluding];
        var counter = 0;
        for (int i = 0; i < collection.Length; i++)
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