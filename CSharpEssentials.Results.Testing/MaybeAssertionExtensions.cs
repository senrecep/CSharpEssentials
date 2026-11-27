using CSharpEssentials.Maybe;

namespace CSharpEssentials.ResultPattern.Testing;

/// <summary>Assertions for <see cref="Maybe{T}"/>. A failed assertion throws <see cref="ResultAssertionException"/>.</summary>
public static class MaybeAssertionExtensions
{
    /// <summary>Asserts that the maybe has a value and returns it.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybe">The maybe under test.</param>
    /// <returns>The value.</returns>
    /// <exception cref="ResultAssertionException">The maybe has no value.</exception>
    public static T ShouldHaveValue<T>(this Maybe<T> maybe)
    {
        if (maybe.HasNoValue)
            throw new ResultAssertionException("Expected a value of type " + typeof(T).Name + ", but the maybe has none.");
        return maybe.Value;
    }

    /// <summary>Asserts that the maybe has no value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybe">The maybe under test.</param>
    /// <exception cref="ResultAssertionException">The maybe has a value.</exception>
    public static void ShouldBeNone<T>(this Maybe<T> maybe)
    {
        if (maybe.HasValue)
            throw new ResultAssertionException("Expected no value, but the maybe has " + AssertionMessages.Case(maybe.Value) + ".");
    }
}
