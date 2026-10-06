using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.EnumsContracts;

/// <summary>A generic container: the generator cannot emit metadata for the enums nested in it.</summary>
/// <typeparam name="T">The type argument.</typeparam>
public static class UnreachableHolder<T>
{
    /// <summary>The type argument.</summary>
    public static Type Argument => typeof(T);

    /// <summary>A [StringEnum] enum without generated metadata.</summary>
    [StringEnum]
    public enum Status
    {
        /// <summary>The first value.</summary>
        First,

        /// <summary>The second value.</summary>
        SecondValue,
    }
}
