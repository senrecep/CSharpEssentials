using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;

Section("Factories and HTTP status");
{
    Error[] errors =
    [
        Error.Validation("Email.Invalid", "Email format is invalid."),
        Error.Unauthorized("Token.Expired", "The token has expired."),
        Error.Forbidden("Order.Forbidden", "You cannot cancel this order."),
        Error.NotFound("User.NotFound", "User was not found."),
        Error.Conflict("User.Duplicate", "User already exists."),
        Error.Domain("Payment.Declined", "The card was declined."),
        Error.Failure("Gateway.Down", "The gateway did not respond."),
        Error.Unexpected("Db.Down", "The database is unreachable."),
        Error.Create("Legacy.Code", "Built with Create.", ErrorType.Unknown),
    ];
    foreach (Error error in errors)
        Console.WriteLine($"{error.Type,-12} {error.Type.ToHttpStatusCode()} {error.Code}");

    Error defaults = Error.NotFound();
    Console.WriteLine($"defaults: {defaults.Code} / {defaults.Description}");
    Console.WriteLine($"422 -> {422.ToErrorType()}, 418 -> {418.ToErrorType()}, Domain number {ErrorType.Domain.ToIntType()}");
}

Section("IError and equality");
{
    IError error = Error.Conflict("User.Duplicate", "User already exists.");
    Console.WriteLine($"{error.Code} {error.Description} {error.Type} {error.NumericType} metadata={error.Metadata is null}");

    Error a = Error.Validation("A", "a", new ErrorMetadata().AddMetadata("x", 1).AddMetadata("y", 2));
    Error b = Error.Validation("A", "a", new ErrorMetadata().AddMetadata("y", 2).AddMetadata("x", 1));
    Console.WriteLine($"equal with metadata in another order: {a == b}");
}

Section("Combining errors");
{
    Error first = Error.Validation("Name.Empty", "Name is required.");
    Error second = Error.Validation("Email.Invalid", "Email is invalid.");
    Error[] pair = first + second;
    Error[] many = Error.CreateMany(first, second, first);
    Error[] single = first;
    Console.WriteLine($"+ gives {pair.Length}, CreateMany gives {many.Length}, implicit gives {single.Length}");
}

Section("Metadata");
{
    ErrorMetadata metadata = new ErrorMetadata()
        .AddMetadata("traceId", "abc-123")
        .AddMetadata("traceId", "ignored")
        .Combine(new ErrorMetadata("path", "/api/users/1"));
    Console.WriteLine($"metadata: {metadata}");

    var field = new ErrorMetadata(ErrorMetadata.FieldKey, "Address.City");
    Console.WriteLine($"GetField: {field.GetField()}");

    ErrorMetadata withStack = ErrorMetadata.CreateEmpty().AddStackTrace();
    Console.WriteLine($"AddStackTrace adds key: {withStack.ContainsKey("stackTrace")}, CreateWithStackTrace too: {ErrorMetadata.CreateWithStackTrace().ContainsKey("stackTrace")}");

    var boom = new InvalidOperationException("Boom");
    Console.WriteLine($"AddException nests under: {string.Join(",", new ErrorMetadata().AddException(boom).Keys)}");
    Console.WriteLine($"AddExceptionDetailed adds: {string.Join(",", new ErrorMetadata().AddExceptionDetailed(boom).Keys)}");
    Console.WriteLine($"CreateWithException: {string.Join(",", ErrorMetadata.CreateWithException(boom).Keys)}, CreateWithExceptionDetailed: {ErrorMetadata.CreateWithExceptionDetailed(boom).Count} keys");
}

Section("Errors from exceptions");
{
    try
    {
        throw new InvalidOperationException("Outer", new TimeoutException("Inner"));
    }
    catch (Exception ex)
    {
        Error error = Error.Exception(ex);
        Console.WriteLine($"{error.Code} {error.Type} {error.Description}");
        var inner = (ErrorMetadata)error.Metadata!["innerException"]!;
        Console.WriteLine($"inner exception type: {inner["exceptionType"]}");

        Error withCode = Error.Exception("Orders.SaveFailed", ex, ErrorType.Failure);
        Console.WriteLine($"{withCode.Code} {withCode.Type} {withCode.Description}");
    }
}

Section("Error catalog");
{
    Console.WriteLine(OrderErrors.EmptyCart.Code);
    Error funds = OrderErrors.InsufficientFunds(120m, 80m);
    Console.WriteLine($"{funds.Code} {funds.Type.ToHttpStatusCode()} required={funds.Metadata!["required"]}");
}

Section("Typed error kinds");
{
    TypedError[] errors =
    [
        UserErrors.NotFound, UserErrors.Deleted, new UserErrors.Locked(7), new DomainError("Plan.Expired", "Plan expired."),
        new UnauthorizedError("Token.Missing", "No token."), new ForbiddenError("Role.Missing", "No role."),
        new FailureError("Gateway.Down", "Gateway down."), new UnexpectedError("Bug", "Unexpected."),
    ];
    foreach (TypedError error in errors)
    {
        string message = error switch
        {
            NotFoundError { Code: UserErrors.Codes.Deleted } => "deleted",
            NotFoundError => "not found",
            UserErrors.Locked locked => $"locked {locked.Id}",
            _ => error.Type.ToString(),
        };
        Console.WriteLine($"{error.GetType().Name,-14} -> {message}");
    }

    Error bridged = UserErrors.NotFound.ToError();
    Console.WriteLine($"ToError: {bridged.Code} {bridged.Type}");

    var invalid = new ValidationError("City.Empty", "City is required.", new ErrorMetadata(ErrorMetadata.FieldKey, "Address.City"));
    Console.WriteLine($"ValidationError.Field: {invalid.Field}");

    NotFoundError left = new("A", "B");
    NotFoundError right = new("A", "B");
    Console.WriteLine($"same code and description equal: {left == right}");
    Console.WriteLine($"derived equals base: {new UserErrors.Locked(1).Equals(new ConflictError(UserErrors.Codes.Locked, "User is locked."))}");

    ErrorKindAttribute? kind = (ErrorKindAttribute?)Attribute.GetCustomAttribute(typeof(OrderLocked), typeof(ErrorKindAttribute));
    OrderLocked structError = new(5);
    Console.WriteLine($"[ErrorKind] on struct: {kind?.Type}, bridged: {Error.Create(structError.Code, structError.Description, structError.Type).Code}");
}

Section("Exceptions that carry errors");
{
    try
    {
        throw new DomainException(Error.Domain("Order.Closed", "Order is closed."));
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"DomainException: {ex.Message} ({ex.Error.Code})");
    }

    try
    {
        throw new EnhancedValidationException([Error.Validation("A", "a"), Error.Validation("B", "b")]);
    }
    catch (EnhancedValidationException ex)
    {
        Console.WriteLine($"EnhancedValidationException: {ex.Message} ({string.Join(", ", ex.Errors.Select(e => e.Code))})");
    }
}

Section("Sentinels");
{
    Console.WriteLine($"{Error.NoErrors.Code}, {Error.NoFirstError.Code}, {Error.NoLastError.Code}, {Error.False.Code}");
}

static void Section(string title) => Console.WriteLine($"{Environment.NewLine}--- {title} ---");

public static class OrderErrors
{
    public static readonly Error EmptyCart = Error.Validation("Order.EmptyCart", "Cannot place an order with an empty cart.");

    public static Error InsufficientFunds(decimal required, decimal available) =>
        Error.Domain(
            "Order.InsufficientFunds",
            $"Payment requires {required} but only {available} is available.",
            new ErrorMetadata().AddMetadata("required", required).AddMetadata("available", available));
}

public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    public sealed record Locked(int Id) : ConflictError(Codes.Locked, "User is locked.");
}

[ErrorKind(ErrorType.Conflict)]
public readonly record struct OrderLocked(int OrderId) : IError
{
    public string Code => "Order.Locked";
    public string Description => $"Order {OrderId} is locked.";
    public ErrorType Type => ErrorType.Conflict;
    public int NumericType => (int)Type;
    public ErrorMetadata? Metadata => null;
}
