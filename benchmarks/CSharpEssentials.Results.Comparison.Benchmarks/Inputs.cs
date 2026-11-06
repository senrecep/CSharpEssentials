namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// The identical value and error every library works with. Errors are cached because each library's idiom allows a
/// static error (CFE uses a constant string, its default error type).
/// </summary>
internal static class Inputs
{
    public const int Value = 42;
    public const string Message = "Order is invalid";

    public static readonly Error CseError = Error.Failure("Order.Invalid", Message);
    public const string CfeError = Message;
    public static readonly Eo.Error ErrorOrError = Eo.Error.Failure("Order.Invalid", Message);
    public static readonly Fr.Error FluentError = new(Message);

    public static Result<int> Cse(bool succeed) => succeed ? Value : CseError;

    public static Cfe.Result<int> CfeOf(bool succeed) => succeed ? Value : Cfe.Result.Failure<int>(CfeError);

    public static Eo.ErrorOr<int> ErrorOr(bool succeed) => succeed ? Value : ErrorOrError;

    public static Fr.Result<int> Fluent(bool succeed) => succeed ? Value : FluentError;
}
