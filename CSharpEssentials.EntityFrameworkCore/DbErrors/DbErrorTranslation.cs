using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.DbErrors;

/// <summary>
/// Entry point for translating persistence exceptions: tries every registered <see cref="IDbErrorTranslator"/> in
/// registration order, then the built-in <see cref="SqlStateErrorTranslator"/>. Register it with
/// <see cref="DbErrorTranslationServiceCollectionExtensions.AddDbErrorTranslation"/>. It is a singleton and keeps the
/// translators it was created with, so translators must be stateless and safe to share.
/// </summary>
public sealed class DbErrorTranslation(IEnumerable<IDbErrorTranslator> translators)
{
    private readonly IDbErrorTranslator[] _translators = [.. translators, new SqlStateErrorTranslator()];

    /// <summary>Returns <see langword="true"/> and the first translated error when any translator recognizes the exception.</summary>
    public bool TryTranslate(Exception exception, out Error error)
    {
        ArgumentNullException.ThrowIfNull(exception);

        foreach (IDbErrorTranslator translator in _translators)
        {
            if (translator.TryTranslate(exception, out error))
                return true;
        }

        error = default;
        return false;
    }

    /// <summary>
    /// Calls <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> and returns the number of written entries, or
    /// the translated error when saving fails with a recognized exception. Unrecognized exceptions are rethrown, unlike
    /// <c>SaveChangesAsResultAsync</c>, which turns every exception into an <see cref="ErrorType.Unknown"/> error.
    /// After a failure the change tracker still holds the changes that were not saved.
    /// </summary>
    public async Task<Result<int>> SaveChangesAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            return await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            if (!TryTranslate(exception, out Error error))
                throw;

            return error;
        }
    }
}
