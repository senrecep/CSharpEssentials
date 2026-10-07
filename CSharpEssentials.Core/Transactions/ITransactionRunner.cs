namespace CSharpEssentials.Transactions;

/// <summary>
/// Runs a unit of work inside a transaction. Implementations start a transaction or join the one already
/// in progress, commit when <c>shouldCommit</c> returns <see langword="true"/>, roll back otherwise or when
/// the work throws, and own any retry policy.
/// </summary>
public interface ITransactionRunner
{
    ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default);
}
