using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wired.OneOf.Functions
{
    internal static class Functions 
    {
        internal static string FormatValue<T>(T value) 
        {
            return typeof(T).FullName + ":" + value == null ? "" : value.ToString();
        }

        internal static string FormatValue<T>(object @this, object @base, T value)
        {
            return ReferenceEquals(@this, value) ?
                    @base.ToString() 
                    :
                    typeof(T).FullName + ":" + value == null ? "" : value.ToString();
        }
}
}
