using CsCheck;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedResultPropertyTests
{
    private const long Iterations = 500;

    private static readonly Gen<Result<int, NotFoundError, ConflictError>> GenResult =
        Gen.Select(Gen.Int[0, 2], Gen.Int, Gen.String, Gen.Bool)
            .Select(sample => Build(sample.Item1, sample.Item2, sample.Item3, sample.Item4));

    private static Result<int, NotFoundError, ConflictError> Build(int slot, int value, string code, bool withMetadata)
    {
        ErrorMetadata? metadata = withMetadata ? new ErrorMetadata("value", value) : null;
        return slot switch
        {
            0 => value,
            1 => new NotFoundError(code, "not found", metadata),
            _ => new ConflictError(code, "conflict", metadata),
        };
    }

    private static int Slot(Result<int, NotFoundError, ConflictError> result) => result.Match(_ => 0, _ => 1, _ => 2);

    [Fact]
    public void Match_Should_ApplyTheSuccessFunctionToTheValue()
    {
        Gen.Int.Sample(value =>
        {
            Result<int, NotFoundError, ConflictError> result = value;

            result.Match(v => (long)v * 3, _ => 0L, _ => 0L).Should().Be((long)value * 3);
        }, iter: Iterations);
    }

    [Fact]
    public void Match_Should_AgreeWithTheSwitchExpressionAndPassTheHeldCase()
    {
        GenResult.Sample(result =>
        {
            object matched = result.Match<object>(v => v, nf => nf, cf => cf);
            object switched = result switch
            {
                int v => v,
                NotFoundError nf => nf,
                ConflictError cf => cf,
            };

            matched.Should().Be(switched);
            matched.Should().Be(result.Value);
            if (result.IsFailure)
                matched.Should().BeSameAs(result.Value);
        }, iter: Iterations);
    }

    [Fact]
    public void ToResult_Should_KeepSuccessValueAndErrorFields()
    {
        GenResult.Sample(result =>
        {
            Result<int> classic = result.ToResult();

            classic.IsSuccess.Should().Be(result.IsSuccess);
            if (result.TryGetValue(out int value))
            {
                classic.Value.Should().Be(value);
                return;
            }

            var typed = (TypedError)result.Value;
            classic.FirstError.Should().Be(typed.ToError());
            classic.FirstError.Metadata.Should().BeSameAs(typed.Metadata);
        }, iter: Iterations);
    }

    [Fact]
    public void Match_Should_AgreeWithToResultMatch()
    {
        GenResult.Sample(result =>
        {
            string typed = result.Match(v => $"ok {v}", nf => nf.Code, cf => cf.Code);
            string classic = result.ToResult().Match(v => $"ok {v}", errors => errors[0].Code);

            typed.Should().Be(classic);
        }, iter: Iterations);
    }

    [Fact]
    public void Reorder_Should_RoundTripAndKeepTheHeldCase()
    {
        GenResult.Sample(result =>
        {
            Result<int, ConflictError, NotFoundError> swapped = result;
            Result<int, NotFoundError, ConflictError> back = swapped;

            back.Value.Should().Be(result.Value);
            Slot(back).Should().Be(Slot(result));
            swapped.Match<object>(v => v, cf => cf, nf => nf).Should().Be(result.Value);
        }, iter: Iterations);
    }

    [Fact]
    public void Widening_Should_KeepMatchFromArity1()
    {
        Gen.Select(Gen.Bool, Gen.Int, Gen.String).Sample((ok, value, code) =>
        {
            Result<int, ConflictError> narrow = ok ? value : new ConflictError(code, "conflict");
            Result<int, NotFoundError, ConflictError> wide = narrow;

            wide.Match<object>(v => v, nf => nf, cf => cf).Should().Be(narrow.Match<object>(v => v, cf => cf));
            wide.IsSuccess.Should().Be(narrow.IsSuccess);
        }, iter: Iterations);
    }
}
