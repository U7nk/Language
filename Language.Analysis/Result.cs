using System;
using System.Diagnostics.CodeAnalysis;

namespace Language.Analysis;

public class Result<TOk, TFail> 
{
    readonly TOk _success;
    readonly TFail? _fail;

    public TOk Ok
    {
        get
        {
            if (IsOk)
            {
                return _success;
            }

            throw new InvalidOperationException("Cannot get success value from failed result");
        }
    }

    public TFail? Error
    {
        get
        {
            if (IsError)
            {
                return _fail;
            }
            
            throw new InvalidOperationException("Cannot get fail value from successful result");
        }
    }

    [MemberNotNullWhen(true, nameof(Ok))]
    public bool IsOk => _success is not null;
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => _fail is not null;

    public Result(TFail fail)
    {
        _fail = fail;
    }

    public Result(TOk success)
    {
        _success = success;
    }
    
    public static implicit operator Result<TOk, TFail>(TOk success) => new(success);
    public static implicit operator Result<TOk, TFail>(TFail fail) => new(fail);
}