#if NET10_0_OR_GREATER
namespace CSharpEssentials.Maybe;

public static class MaybeExtensionMembers
{
    extension<T>(Maybe<T> maybe)
    {
        [Obsolete("Use HasNoValue.")]
        public bool IsNone => !maybe.HasValue;
    }
}
#endif
