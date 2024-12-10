namespace CSharpEssentials;
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
    public static IEnumerable<string?> GetInnerExceptionsMessages(this Exception ex) =>
        ex.GetInnerExceptions().Select(x => x.Message);
}
