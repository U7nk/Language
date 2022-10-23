using System;

namespace Language.Analysis.Exceptions;

[Serializable]
public class WiredException : Exception
{
    public WiredException() { }
    public WiredException(string message) : base(message) { }
    public WiredException(string message, Exception inner) : base(message, inner) { }
    protected WiredException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}