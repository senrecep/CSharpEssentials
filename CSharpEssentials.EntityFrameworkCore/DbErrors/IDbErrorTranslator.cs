using CSharpEssentials.Errors;

namespace CSharpEssentials.EntityFrameworkCore.DbErrors;

/// <summary>
/// Turns a persistence exception into an <see cref="Error"/>. Register implementations with
/// <see cref="DbErrorTranslationServiceCollectionExtensions.AddDbErrorTranslator{TTranslator}"/>; they are tried in
/// registration order before the built-in <see cref="SqlStateErrorTranslator"/>, and the first match wins.
/// </summary>
public interface IDbErrorTranslator
{
    /// <summary>Returns <see langword="true"/> and the translated error when this translator recognizes the exception.</summary>
    bool TryTranslate(Exception exception, out Error error);
}
