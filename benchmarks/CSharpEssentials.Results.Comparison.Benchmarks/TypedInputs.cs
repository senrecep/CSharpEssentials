namespace CSharpEssentials.Results.Comparison.Benchmarks;

/// <summary>
/// The cases every typed comparison works with: the success value 42, a cached not-found error, a conflict error built on
/// each call and a cached validation error. Each library folds a case to the same status code (42, 404, 409 or 400), so the
/// parity checks compare integers. Single-error-type libraries carry the kind the way they document it: CSE
/// <see cref="Result{TValue}"/> and ErrorOr through the error's type, CFE through a <see cref="TypedError"/> error slot,
/// FluentResults through an error subclass.
/// </summary>
internal static class TypedInputs
{
    public const int Batch = 1024;

    public const string Success = "Success";
    public const string NotFound = "NotFound";
    public const string Validation = "Validation";

    public const int NotFoundStatus = 404;
    public const int ConflictStatus = 409;
    public const int ValidationStatus = 400;

    public const string NotFoundCode = "Order.NotFound";
    public const string NotFoundText = "Order 42 was not found";
    public const string ConflictCode = "Order.Conflict";
    public const string ConflictText = "Order 42 was changed";
    public const string InvalidCode = "Order.Invalid";

    public static readonly NotFoundError Missing = new(NotFoundCode, NotFoundText);
    public static readonly ValidationError Invalid = new(InvalidCode, Inputs.Message);

    public static readonly Error CseMissing = Error.NotFound(NotFoundCode, NotFoundText);
    public static readonly Error CseInvalid = Error.Validation(InvalidCode, Inputs.Message);

    public static readonly Eo.Error ErrorOrMissing = Eo.Error.NotFound(NotFoundCode, NotFoundText);
    public static readonly Eo.Error ErrorOrInvalid = Eo.Error.Validation(InvalidCode, Inputs.Message);

    public static readonly Fr.IError FluentMissing = new FluentNotFound(NotFoundText);
    public static readonly Fr.IError FluentInvalidError = new FluentInvalid(Inputs.Message);

    public static Typed Cse(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };

    public static Result<int> CseResult(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => CseMissing,
        _ => CseInvalid,
    };

    public static TypedAny Any(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };

    public static TypedOneOf OneOf(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };

    public static OrderOutcome Dunet(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };

    public static Eo.ErrorOr<int> ErrorOr(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => ErrorOrMissing,
        _ => ErrorOrInvalid,
    };

    public static Cfe.Result<int, TypedError> Cfe(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };

    public static Fr.Result<int> Fluent(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Fr.Result.Fail<int>(FluentMissing),
        _ => Fr.Result.Fail<int>(FluentInvalidError),
    };

#if NET11_0_OR_GREATER
    public static OrderUnion Native(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => Missing,
        _ => Invalid,
    };
#endif

    public static int Expected(string kind) => kind switch
    {
        Success => Inputs.Value,
        NotFound => NotFoundStatus,
        _ => ValidationStatus,
    };

    public static int Status(ErrorType type) => type switch
    {
        ErrorType.NotFound => NotFoundStatus,
        ErrorType.Conflict => ConflictStatus,
        _ => ValidationStatus,
    };

    public static int Status(Eo.ErrorType type) => type switch
    {
        Eo.ErrorType.NotFound => NotFoundStatus,
        Eo.ErrorType.Conflict => ConflictStatus,
        _ => ValidationStatus,
    };

    public static int Status(TypedError error) => error switch
    {
        NotFoundError => NotFoundStatus,
        ConflictError => ConflictStatus,
        _ => ValidationStatus,
    };

    public static int Status(Fr.IError error) => error switch
    {
        FluentNotFound => NotFoundStatus,
        FluentConflict => ConflictStatus,
        _ => ValidationStatus,
    };

    public static int Read(Typed r) => r.Match(static v => v, static _ => NotFoundStatus, static _ => ConflictStatus, static _ => ValidationStatus);

    public static int Read(Result<int> r) => r.IsSuccess ? r.Value : Status(r.Errors[0].Type);

    public static int Read(TypedAny r) => r.Match(static v => v, static _ => NotFoundStatus, static _ => ConflictStatus, static _ => ValidationStatus).Result;

    public static int Read(TypedOneOf r) => r.Match(static v => v, static _ => NotFoundStatus, static _ => ConflictStatus, static _ => ValidationStatus);

    public static int Read(OrderOutcome r) => r.Match(static ok => ok.Value, static _ => NotFoundStatus, static _ => ConflictStatus, static _ => ValidationStatus);

    public static int Read(Eo.ErrorOr<int> r) => r.IsError ? Status(r.FirstError.Type) : r.Value;

    public static int Read(Cfe.Result<int, TypedError> r) => r.IsSuccess ? r.Value : Status(r.Error);

    public static int Read(Fr.Result<int> r) => r.IsSuccess ? r.Value : Status(r.Errors[0]);

#if NET11_0_OR_GREATER
    public static int Read(OrderUnion r) => r switch
    {
        int v => v,
        NotFoundError => NotFoundStatus,
        ConflictError => ConflictStatus,
        ValidationError => ValidationStatus,
    };
#endif
}
