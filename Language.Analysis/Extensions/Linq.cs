using System;
using System.Collections.Generic;

namespace Language.Analysis.Extensions;

public static class Linq
{
    
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            return item;
        }

        return Option.None;
    }
    
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        return Option.None;
    }
    
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> enumerable)
    {
        var found = false;
        T? value = default;
        foreach (var item in enumerable)
        {
            if (found)
            {
                throw new InvalidOperationException("Sequence contains more than one element");
            }

            found = true;
            value = item;
        }

        return found ? value : Option.None;
    }
    
    public static Option<T> SingleOrNone<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
    {
        var found = false;
        T? value = default;
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                if (found)
                {
                    throw new InvalidOperationException("Sequence contains more than one element");
                }

                found = true;
                value = item;
            }
        }

        return found ? value : Option.None;
    }
}