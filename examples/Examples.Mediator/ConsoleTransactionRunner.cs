using CSharpEssentials.Transactions;

namespace Examples.Mediator;

/// <summary>Stands in for a database transaction and prints whether the work commits or rolls back.</summary>
internal sealed class ConsoleTransactionRunner : ITransactionRunner
{
    public async ValueTask<T> ExecuteAsync<T>(
        Func<CancellationToken, ValueTask<T>> work,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("BEGIN");
        try
        {
            T result = await work(cancellationToken);
            Console.WriteLine(shouldCommit(result) ? "COMMIT" : "ROLLBACK (failed Result)");
            return result;
        }
        catch
        {
            Console.WriteLine("ROLLBACK (exception)");
            throw;
        }
    }
}
