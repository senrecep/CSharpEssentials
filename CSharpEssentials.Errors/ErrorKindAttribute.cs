namespace CSharpEssentials.Errors;

/// <summary>
/// Declares the <see cref="ErrorType"/> of an error type, so tools can read the kind from the type alone
/// (for example to list response status codes). Derived types inherit it. Works on classes and structs.
/// </summary>
/// <remarks>
/// Do not redeclare it with a different value on a type derived from a typed error kind: the kind's
/// <see cref="IError.Type"/> stays fixed, so the attribute and the runtime type would disagree.
/// </remarks>
/// <param name="type">The error type that values of the annotated type carry.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
public sealed class ErrorKindAttribute(ErrorType type) : Attribute
{
    /// <summary>
    /// Gets the declared error type.
    /// </summary>
    public ErrorType Type { get; } = type;
}
