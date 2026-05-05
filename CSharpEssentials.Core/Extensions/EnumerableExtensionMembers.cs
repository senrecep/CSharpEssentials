#if NET10_0_OR_GREATER
namespace CSharpEssentials;

public static class EnumerableExtensionMembers
{
    extension<T>(IEnumerable<T> source)
    {
        public bool IsEmpty => !source.Any();
    }
}
#endif
