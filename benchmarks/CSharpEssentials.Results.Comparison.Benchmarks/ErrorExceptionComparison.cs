using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
public class ErrorExceptionComparison
{
    private const string Boom = "boom";

    private Exception _exception = new InvalidOperationException(Boom);

    [Params(true, false)]
    public bool Thrown { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _exception = Thrown ? Throw() : new InvalidOperationException(Boom);

        Error cse = CSharpEssentials();
        Error failure = CSharpEssentials_Failure();
        Parity.Same(nameof(ErrorExceptionComparison),
            ("Expected", Boom),
            (nameof(CSharpEssentials), cse.Description),
            (nameof(CSharpEssentials_Failure), failure.Description),
            (nameof(ErrorOr), ErrorOr().Description),
            (nameof(FluentResults), FluentResults().Message));
        Parity.Same(nameof(ErrorExceptionComparison) + ".Code",
            ("Expected", nameof(InvalidOperationException) + "|" + ErrorType.Failure),
            (nameof(CSharpEssentials), cse.Code + "|" + cse.Type),
            (nameof(CSharpEssentials_Failure), failure.Code + "|" + failure.Type));
        Parity.Same(nameof(ErrorExceptionComparison) + ".Metadata",
            ("Expected", "exceptionMessage,exceptionStackTrace,exceptionType"),
            (nameof(CSharpEssentials), string.Join(",", cse.Metadata!.Keys.Order(StringComparer.Ordinal))));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#converting-from-exceptions
    [Benchmark(Baseline = true)]
    public Error CSharpEssentials() => Error.Exception(_exception);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#creating-errors
    [Benchmark]
    public Error CSharpEssentials_Failure() => Error.Failure(_exception.GetType().Name, _exception.Message);

    // https://github.com/amantinband/error-or/blob/v2.1.1/README.md#replace-throwing-exceptions-with-errorort
    [Benchmark]
    public Eo.Error ErrorOr() => Eo.Error.Failure(_exception.GetType().Name, _exception.Message);

    // https://github.com/altmann/FluentResults/blob/v4.0/README.md#try
    // https://github.com/altmann/FluentResults/blob/v4.0/src/FluentResults/Reasons/ExceptionalError.cs
    [Benchmark]
    public Fr.ExceptionalError FluentResults() => new(_exception);

    private static InvalidOperationException Throw()
    {
        try
        {
            throw new InvalidOperationException(Boom);
        }
        catch (InvalidOperationException ex)
        {
            return ex;
        }
    }
}
