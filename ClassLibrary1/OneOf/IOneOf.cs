using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wired.OneOf
{
    public interface IOneOf
    {
        object Value { get; }
        int Index { get; }
    }
}
