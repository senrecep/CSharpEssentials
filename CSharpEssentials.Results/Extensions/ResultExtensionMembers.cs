#if NET10_0_OR_GREATER
namespace CSharpEssentials.ResultPattern;

public static class ResultExtensionMembers
{
    extension<T>(Result<T> result)
    {
        public T? ValueOrDefault => result.IsSuccess ? result.Value : default;
    }

    extension<T>(Result<T> left)
    {
        public static Result<T> operator |(Result<T> l, Result<T> r)
            => l.IsSuccess ? l : r;
    }
}
#endif
