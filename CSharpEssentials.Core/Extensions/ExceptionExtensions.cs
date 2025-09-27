using System.Runtime.CompilerServices;

namespace CSharpEssentials.Core;

public static class ExceptionExtensions
{
    public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
    {
        Exception? innerException = ex;
        while (innerException is not null)
        {
            yield return innerException;
            innerException = innerException.InnerException;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string?> GetInnerExceptionsMessages(this Exception ex) =>
        ex.GetInnerExceptions().Select(x => x.Message);
}
