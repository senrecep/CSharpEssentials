using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class NeverTranslator : IDbErrorTranslator
{
    public bool TryTranslate(Exception exception, out Error error)
    {
        error = default;
        return false;
    }
}
