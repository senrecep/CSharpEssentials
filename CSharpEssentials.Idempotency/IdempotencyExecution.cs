namespace CSharpEssentials.Idempotency;

/// <summary>The answer of <see cref="IIdempotencyExecutor.ExecuteAsync{TResult}"/>.</summary>
/// <typeparam name="TResult">The result of the operation.</typeparam>
public readonly record struct IdempotencyExecution<TResult>
{
    /// <param name="status">Whether the operation ran, was replayed or was stopped.</param>
    /// <param name="value">The result of the run or of the replay.</param>
    public IdempotencyExecution(IdempotencyExecutionStatus status, TResult? value)
    {
        Status = status;
        Value = value;
    }

    /// <summary>Whether the operation ran, was replayed or was stopped.</summary>
    public IdempotencyExecutionStatus Status { get; }

    /// <summary>
    /// The result of the run or of the replay; <see langword="default"/> for <see cref="IdempotencyExecutionStatus.InFlight"/>
    /// and <see cref="IdempotencyExecutionStatus.KeyReused"/>.
    /// </summary>
    public TResult? Value { get; }

    /// <summary><see langword="true"/> when <see cref="Value"/> holds a result, run now or replayed.</summary>
    public bool HasValue => Status is IdempotencyExecutionStatus.Executed or IdempotencyExecutionStatus.Replayed;
}
