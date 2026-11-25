using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation;

/// <summary>
/// Writes the <see cref="ErrorMetadata.FieldKey"/> entry on the errors a rule chain records.
/// </summary>
internal static class FieldMetadata
{
    /// <summary>
    /// New metadata holding only <paramref name="field"/> under <see cref="ErrorMetadata.FieldKey"/>.
    /// </summary>
    internal static ErrorMetadata Create(string field) => new(ErrorMetadata.FieldKey, field);

    /// <summary>
    /// Returns <paramref name="error"/> with <paramref name="field"/> under <see cref="ErrorMetadata.FieldKey"/>,
    /// replacing any field it had.
    /// </summary>
    internal static Error Replace(Error error, string field)
    {
        ErrorMetadata metadata = error.Metadata is null ? [] : new ErrorMetadata(error.Metadata);
        metadata[ErrorMetadata.FieldKey] = field;
        return Error.Create(error.Code, error.Description, error.Type, metadata);
    }
}
