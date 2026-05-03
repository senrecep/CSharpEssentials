using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Errors Example");
Console.WriteLine("========================================\n");

// ============================================================================
// ERROR CREATION
// ============================================================================
Console.WriteLine("--- Error Creation ---");

Error notFound = Error.NotFound("User.NotFound", "User with id 123 was not found");
Console.WriteLine($"NotFound: Code={notFound.Code}, Description={notFound.Description}, Type={notFound.Type}");

Error validation = Error.Validation("Email.Invalid", "Email address is not valid");
Console.WriteLine($"Validation: Code={validation.Code}, Type={validation.Type}");

Error conflict = Error.Conflict("User.Duplicate", "A user with this email already exists");
Console.WriteLine($"Conflict: Code={conflict.Code}, Type={conflict.Type}");

Error unauthorized = Error.Unauthorized("Auth.Token", "Invalid or expired token");
Console.WriteLine($"Unauthorized: Code={unauthorized.Code}, Type={unauthorized.Type}");

Error forbidden = Error.Forbidden("Auth.Permission", "You do not have permission to access this resource");
Console.WriteLine($"Forbidden: Code={forbidden.Code}, Type={forbidden.Type}");

Error unexpected = Error.Unexpected("System.Error", "An unexpected error occurred");
Console.WriteLine($"Unexpected: Code={unexpected.Code}, Type={unexpected.Type}");

Error failure = Error.Failure("Payment.Failed", "Payment could not be processed");
Console.WriteLine($"Failure: Code={failure.Code}, Type={failure.Type}");
Console.WriteLine();

// ============================================================================
// ERROR TYPE CONVERSION
// ============================================================================
Console.WriteLine("--- Error Type to HTTP Status ---");

Error[] errors = { notFound, validation, conflict, unauthorized, forbidden, unexpected };
foreach (Error error in errors)
{
    Console.WriteLine($"{error.Type} -> HTTP {error.Type.ToHttpStatusCode()}");
}
Console.WriteLine();

// ============================================================================
// ERROR METADATA
// ============================================================================
Console.WriteLine("--- Error Metadata ---");

ErrorMetadata metadata = new ErrorMetadata()
    .AddMetadata("TraceId", Guid.NewGuid().ToString())
    .AddMetadata("Timestamp", DateTime.UtcNow.ToString("O"))
    .AddMetadata("RequestPath", "/api/users/123");

Error withMetadata = Error.NotFound("User.NotFound", "User not found", metadata);
Console.WriteLine($"Error with metadata: {withMetadata.Code}");
if (withMetadata.Metadata is not null)
{
    foreach (System.Collections.Generic.KeyValuePair<string, object?> item in withMetadata.Metadata)
    {
        Console.WriteLine($"  {item.Key}: {item.Value}");
    }
}
Console.WriteLine();

// ============================================================================
// MULTIPLE ERRORS
// ============================================================================
Console.WriteLine("--- Multiple Errors ---");

Error[] multipleErrors =
{
    Error.Validation("Name.Empty", "Name is required"),
    Error.Validation("Email.Invalid", "Email is invalid"),
    Error.Validation("Age.Range", "Age must be between 18 and 120")
};

Console.WriteLine($"Total validation errors: {multipleErrors.Length}");
foreach (Error err in multipleErrors)
{
    Console.WriteLine($"  - {err.Code}: {err.Description}");
}
Console.WriteLine();

// ============================================================================
// EXCEPTION-BASED ERROR
// ============================================================================
Console.WriteLine("--- Exception-Based Error ---");

try
{
    throw new InvalidOperationException("Database connection failed");
}
catch (Exception ex)
{
    Error exceptionError = Error.Exception("Database.Error", ex);
    Console.WriteLine($"Exception Error: {exceptionError.Code} - {exceptionError.Description}");
}
Console.WriteLine();

// ============================================================================
// DOMAIN EXCEPTION
// ============================================================================
Console.WriteLine("--- DomainException ---");

try
{
    Error domainError = Error.Validation("Order.Invalid", "Order total must be greater than zero");
    throw new DomainException(domainError);
}
catch (DomainException dex)
{
    Console.WriteLine($"DomainException caught: {dex.Message}");
    Console.WriteLine($"  Error Code: {dex.Error.Code}");
    Console.WriteLine($"  Error Type: {dex.Error.Type}");
}
Console.WriteLine();

// ============================================================================
// ENHANCED VALIDATION EXCEPTION
// ============================================================================
Console.WriteLine("--- EnhancedValidationException ---");

try
{
    Error[] validationErrors =
    {
        Error.Validation("Name.Empty", "Name is required"),
        Error.Validation("Email.Invalid", "Email format is invalid")
    };
    throw new EnhancedValidationException(validationErrors);
}
catch (EnhancedValidationException vex)
{
    Console.WriteLine($"EnhancedValidationException caught: {vex.Message}");
    Console.WriteLine($"  Total errors: {vex.Errors.Length}");
    foreach (Error err in vex.Errors)
    {
        Console.WriteLine($"    - {err.Code}: {err.Description}");
    }
}
Console.WriteLine();

// ============================================================================
// IERROR INTERFACE
// ============================================================================
Console.WriteLine("--- IError Interface ---");

static void LogError(IError error)
{
    Console.WriteLine($"[LOG] {error.Code}: {error.Description} (Type: {error.Type})");
}

LogError(notFound);
LogError(validation);
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");
