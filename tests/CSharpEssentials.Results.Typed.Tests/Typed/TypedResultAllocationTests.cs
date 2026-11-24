using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedResultAllocationTests
{
    private const int Iterations = 10_000;

    private static Result<int, NotFoundError, ConflictError> Get(int value) => value;

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
}
