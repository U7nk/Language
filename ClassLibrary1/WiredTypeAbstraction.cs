using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wired
{
    public abstract class WiredType { }

    public class DelegateWiredType : WiredType
    {
        public int ParametersCount { get; set; }
        public DelegateWiredType(int parametersCount)
        {
            this.ParametersCount = parametersCount;
        }
    }

    public class NamespaceWiredType : WiredType
    {
    }
}
