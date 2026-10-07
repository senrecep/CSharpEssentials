using CSharpEssentials.EntityFrameworkCore.DbErrors;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Tests.EntityFrameworkCore.DbErrors;

public sealed class RecordingTranslator(Error? result) : IDbErrorTranslator
{
    public int Calls { get; private set; }

    public bool TryTranslate(Exception exception, out Error error)
    {
        Calls++;
        error = result ?? default;
        return result is not null;
    }
}
