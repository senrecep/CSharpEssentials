using System;

namespace CSharpEssentials.Any;

public enum AnyActionStatus
{
    NotExecuted,
    Executed
}

public readonly record struct AnyActionResult<TResult>(AnyActionStatus Status, TResult? Result)
{
    public static implicit operator AnyActionResult<TResult>(TResult? result) => new(AnyActionStatus.Executed, result);
    public static implicit operator AnyActionResult<TResult>(AnyActionStatus status) => new(status, default);
}