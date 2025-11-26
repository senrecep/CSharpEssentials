# CSharpEssentials.Errors

`CSharpEssentials.Errors` provides a structured, type-safe error handling system for .NET applications. It encapsulates errors as data rather than exceptions, promoting the Result pattern and making failure flows explicit and manageable.

## 🚀 Features

- **Structured Errors**: `Error` struct with unique codes, descriptions, and types (NotFound, Validation, Conflict, etc.).
- **Rich Metadata**: Attach additional context (`ErrorMetadata`) to errors.
- **Standard Error Types**: predefined types like `Failure`, `Unexpected`, `Validation`, `Conflict`, `NotFound`, `Unauthorized`, `Forbidden`.
- **Exception Integration**: Convert exceptions to `Error` objects or wrap `Error` objects in `DomainException`.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Errors
```

## 🛠 Usage

### 1. Creating Errors

Use the static factory methods on the `Error` struct to create standard errors.

```csharp
using CSharpEssentials.Errors;

// Common error types
var notFound = Error.NotFound("User.NotFound", "The user with the specified ID was not found.");
var validation = Error.Validation("User.EmailInvalid", "The email address is in an invalid format.");
var conflict = Error.Conflict("User.AlreadyExists", "A user with this email already exists.");
var failure = Error.Failure("Payment.Failed", "The payment gateway rejected the transaction.");
```

### 2. Error Metadata

You can attach extra information to an error for debugging or client responses.

```csharp
var metadata = new ErrorMetadata 
{
    { "RetryCount", 3 },
    { "Gateway", "Stripe" }
};

var error = Error.Failure(
    code: "Payment.Failed", 
    description: "Transaction failed", 
    metadata: metadata
);
```

### 3. Converting Exceptions to Errors

Capture exception details into a structured error.

```csharp
try 
{
    // ... risky code ...
}
catch (Exception ex)
{
    var error = Error.Exception(
        code: "System.Unhandled", 
        exception: ex
    );
    // error.Description will be ex.Message
    // error.Metadata will contain exception details
}
```

### 4. Domain Exceptions

If you must throw an exception but want to keep the structured error format (e.g., to be caught by a global handler), use `DomainException`.

```csharp
using CSharpEssentials.Exceptions;

public void DoSomething()
{
    var error = Error.Validation("Input.Invalid", "Value cannot be negative");
    throw new DomainException(error);
}
```

### 5. Error Equality

Errors are value types and check for equality based on their content (Code, Description, Type, Metadata).

```csharp
var e1 = Error.NotFound("Item.Missing");
var e2 = Error.NotFound("Item.Missing");

bool areEqual = (e1 == e2); // True
```
