﻿using System;

namespace Language.Analysis.Exceptions;

[Serializable]
public class TypeResolverException : WiredException
{
    public TypeResolverException() { }
    public TypeResolverException(string message) : base(message) { }
    public TypeResolverException(string message, Exception inner) : base(message, inner) { }
    protected TypeResolverException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}