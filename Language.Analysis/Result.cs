using System;
using System.Diagnostics.CodeAnalysis;

namespace Language.Analysis;

public class Result<TSuccess, TFail> 
{
    readonly TSuccess? _success;
    readonly TFail? _fail;

    public TSuccess? Success
    {
        get
        {
            if (IsSuccess)
            {
                return _success;
            }

            throw new InvalidOperationException("Cannot get success value from failed result");
        }
    }

    public TFail? Fail
    {
        get
        {
            if (IsFail)
            {
                return _fail;
            }
            
            throw new InvalidOperationException("Cannot get fail value from successful result");
        }
    }

    [MemberNotNullWhen(true, nameof(Success))]
    public bool IsSuccess => _success is not null;
    [MemberNotNullWhen(true, nameof(Fail))]
    public bool IsFail => _fail is not null;

    public Result(TFail fail)
    {
        _fail = fail;
    }

    public Result(TSuccess success)
    {
        _success = success;
    }
    
    public static implicit operator Result<TSuccess, TFail>(TSuccess success) => new(success);
    public static implicit operator Result<TSuccess, TFail>(TFail fail) => new(fail);
}