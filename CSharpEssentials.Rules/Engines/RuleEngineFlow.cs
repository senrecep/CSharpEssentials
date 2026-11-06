using System.Buffers;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    private static TRule? LinearNext<TRule>(bool isFailure, TRule? next) where TRule : class =>
        isFailure ? null : next;

    private static TRule? ConditionalBranch<TRule>(bool isFailure, TRule? success, TRule? failure) where TRule : class =>
        isFailure ? failure : success;

    private static Error[][] RentFailures(int length) => ArrayPool<Error[]>.Shared.Rent(length);

    private static void ReturnFailures(Error[][] failures, int count)
    {
        Array.Clear(failures, 0, count);
        ArrayPool<Error[]>.Shared.Return(failures);
    }

    private static Error[] FlattenFailures(Error[][] failures, int count)
    {
        if (count == 1)
            return failures[0];

        int total = 0;
        for (int i = 0; i < count; i++)
            total += failures[i].Length;

        var errors = new Error[total];
        int offset = 0;
        for (int i = 0; i < count; i++)
        {
            Array.Copy(failures[i], 0, errors, offset, failures[i].Length);
            offset += failures[i].Length;
        }
        return errors;
    }

    private static bool IsCallerCancellation(Exception exception, CancellationToken cancellationToken) =>
        exception is OperationCanceledException && cancellationToken.IsCancellationRequested;
}
