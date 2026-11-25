using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedResultAllocationTests
{
    private const int Iterations = 10_000;

    private static readonly ConflictError Busy = new("Order.Busy", "Order is busy.");
    private static readonly ValidationError Invalid = new(UserErrors.Codes.Invalid, "User is invalid.");

    private static Result<int, NotFoundError, ConflictError> Get(int value) => value;

    private static Result<int, ConflictError> Lock(int value) => value % 5 == 0 ? Busy : value;

    private static long Run()
    {
        long sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            Result<int, NotFoundError, ConflictError> result = Get(i);
            sum += result switch
            {
                int held => held,
                NotFoundError => -1,
                ConflictError => -2,
            };
            sum += result.Match(static value => value, static _ => -1, static _ => -2);
            if (result.TryGetValue(out int got) && result.IsSuccess)
                sum += got;
            Result<int, ConflictError, NotFoundError> swapped = result;
            sum += swapped.Match(static value => value, static _ => -1, static _ => -2);
        }

        return sum;
    }

    [Fact]
    public void SuccessPath_Should_NotAllocate_For_AValueTypeResult()
    {
        Run();

        long before = GC.GetAllocatedBytesForCurrentThread();
        long sum = Run();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        sum.Should().BeGreaterThan(0);
        allocated.Should().Be(0);
    }

    private static long RunCombinators()
    {
        long sum = 0;
        for (int i = 0; i < Iterations; i++)
        {
            Result<int, NotFoundError> source = i % 3 == 0 ? UserErrors.NotFound : i;
            sum += source
                .Map(static value => value + 1)
                .Ensure(static value => value > 0, UserErrors.Deleted)
                .BindWiden(static value => Lock(value))
                .Bind<int>(static value => value * 2)
                .MapError(static (ConflictError _) => Invalid)
                .Recover(static (NotFoundError _) => -1)
                .Recover(static (ValidationError _) => -2);
        }

        return sum;
    }

    [Fact]
    public void Combinators_Should_NotAllocate_For_AValueTypeResultWithPredefinedErrors()
    {
        RunCombinators();

        long before = GC.GetAllocatedBytesForCurrentThread();
        long sum = RunCombinators();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        sum.Should().NotBe(0);
        allocated.Should().Be(0);
    }
}
