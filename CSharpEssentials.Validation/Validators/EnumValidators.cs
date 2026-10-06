using System.Globalization;
using CSharpEssentials.Enums;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Validation.Validators;

/// <summary>
/// Extension validators for enum properties. Errors use code <c>enum.invalid</c> or <c>enum.not_allowed</c> and the
/// <see cref="EnumValueError.Message"/> text that binding errors use. Enums with generated metadata (<see cref="StringEnumAttribute"/>)
/// list their input wire names; other enums list their C# member names. A <see langword="null"/> nullable value passes.
/// </summary>
public static class EnumValidators
{
    /// <summary>Error code for a value that is not a defined member or flags combination.</summary>
    public const string InvalidCode = "enum.invalid";

    /// <summary>Error code for a defined value outside the allowed subset.</summary>
    public const string NotAllowedCode = "enum.not_allowed";

    /// <summary>
    /// Fails if the value is not a defined member. A <see cref="FlagsAttribute"/> enum also accepts any combination of defined flags.
    /// </summary>
    public static RuleChain<T, TEnum> IsDefinedEnum<T, TEnum>(this RuleChain<T, TEnum> chain, string? message = null)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && !IsDefined(chain.Value))
            chain.AddError(Invalid(chain.Value, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value is not a defined member. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum> IsDefinedEnum<T, TEnum>(this RuleChain<T, TEnum> chain, Error error)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && !IsDefined(chain.Value))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is present and not a defined member.</summary>
    public static RuleChain<T, TEnum?> IsDefinedEnum<T, TEnum>(this RuleChain<T, TEnum?> chain, string? message = null)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && chain.Value is TEnum value && !IsDefined(value))
            chain.AddError(Invalid(value, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value is present and not a defined member. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum?> IsDefinedEnum<T, TEnum>(this RuleChain<T, TEnum?> chain, Error error)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && chain.Value is TEnum value && !IsDefined(value))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is not equal to one of <paramref name="allowed"/>. The error lists the allowed values.</summary>
    public static RuleChain<T, TEnum> IsOneOf<T, TEnum>(this RuleChain<T, TEnum> chain, params TEnum[] allowed)
        where TEnum : struct, Enum =>
        chain.IsOneOf(allowed, message: null);

    /// <summary>Fails if the value is not equal to one of <paramref name="allowed"/>.</summary>
    public static RuleChain<T, TEnum> IsOneOf<T, TEnum>(this RuleChain<T, TEnum> chain, TEnum[] allowed, string? message)
        where TEnum : struct, Enum
    {
        _ = allowed ?? throw new ArgumentNullException(nameof(allowed));
        if (!chain.HasFailed && Array.IndexOf(allowed, chain.Value) < 0)
            chain.AddError(NotAllowed(chain.Value, allowed, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value is not equal to one of <paramref name="allowed"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum> IsOneOf<T, TEnum>(this RuleChain<T, TEnum> chain, TEnum[] allowed, Error error)
        where TEnum : struct, Enum
    {
        _ = allowed ?? throw new ArgumentNullException(nameof(allowed));
        if (!chain.HasFailed && Array.IndexOf(allowed, chain.Value) < 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is present and not equal to one of <paramref name="allowed"/>.</summary>
    public static RuleChain<T, TEnum?> IsOneOf<T, TEnum>(this RuleChain<T, TEnum?> chain, params TEnum[] allowed)
        where TEnum : struct, Enum =>
        chain.IsOneOf(allowed, message: null);

    /// <summary>Fails if the value is present and not equal to one of <paramref name="allowed"/>.</summary>
    public static RuleChain<T, TEnum?> IsOneOf<T, TEnum>(this RuleChain<T, TEnum?> chain, TEnum[] allowed, string? message)
        where TEnum : struct, Enum
    {
        _ = allowed ?? throw new ArgumentNullException(nameof(allowed));
        if (!chain.HasFailed && chain.Value is TEnum value && Array.IndexOf(allowed, value) < 0)
            chain.AddError(NotAllowed(value, allowed, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value is present and not equal to one of <paramref name="allowed"/>. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum?> IsOneOf<T, TEnum>(this RuleChain<T, TEnum?> chain, TEnum[] allowed, Error error)
        where TEnum : struct, Enum
    {
        _ = allowed ?? throw new ArgumentNullException(nameof(allowed));
        if (!chain.HasFailed && chain.Value is TEnum value && Array.IndexOf(allowed, value) < 0)
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value has a bit that no defined member sets. Zero always passes.</summary>
    public static RuleChain<T, TEnum> HasOnlyDefinedFlags<T, TEnum>(this RuleChain<T, TEnum> chain, string? message = null)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && !HasOnlyDefinedBits(chain.Value))
            chain.AddError(Invalid(chain.Value, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value has a bit that no defined member sets. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum> HasOnlyDefinedFlags<T, TEnum>(this RuleChain<T, TEnum> chain, Error error)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && !HasOnlyDefinedBits(chain.Value))
            chain.AddError(error);
        return chain;
    }

    /// <summary>Fails if the value is present and has a bit that no defined member sets.</summary>
    public static RuleChain<T, TEnum?> HasOnlyDefinedFlags<T, TEnum>(this RuleChain<T, TEnum?> chain, string? message = null)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && chain.Value is TEnum value && !HasOnlyDefinedBits(value))
            chain.AddError(Invalid(value, chain.PropertyName, message));
        return chain;
    }

    /// <summary>Fails if the value is present and has a bit that no defined member sets. Uses <paramref name="error"/> directly.</summary>
    public static RuleChain<T, TEnum?> HasOnlyDefinedFlags<T, TEnum>(this RuleChain<T, TEnum?> chain, Error error)
        where TEnum : struct, Enum
    {
        if (!chain.HasFailed && chain.Value is TEnum value && !HasOnlyDefinedBits(value))
            chain.AddError(error);
        return chain;
    }

    private static bool IsDefined<TEnum>(TEnum value) where TEnum : struct, Enum =>
        EnumMetadata.TryGet(out EnumInfo<TEnum>? info)
            ? info.IsDefined(value)
            : PlainEnum<TEnum>.Contains(value);

    private static bool HasOnlyDefinedBits<TEnum>(TEnum value) where TEnum : struct, Enum =>
        EnumMetadata.TryGet(out EnumInfo<TEnum>? info)
            ? (info.ToRawValue(value) & ~info.DefinedMask) == 0
            : (PlainEnum<TEnum>.ToRaw(value) & ~PlainEnum<TEnum>.Mask) == 0;

    private static Error Invalid<TEnum>(TEnum value, string propertyName, string? message) where TEnum : struct, Enum =>
        Error.Validation(InvalidCode, message ?? CreateError(value, propertyName).Message);

    private static Error NotAllowed<TEnum>(TEnum value, TEnum[] allowed, string propertyName, string? message) where TEnum : struct, Enum
    {
        if (message is not null)
            return Error.Validation(NotAllowedCode, message);
        string[] allowedValues = new string[allowed.Length];
        for (int i = 0; i < allowed.Length; i++)
            allowedValues[i] = Text(allowed[i]);
        return Error.Validation(NotAllowedCode, new EnumValueError(typeof(TEnum), Text(value), allowedValues, propertyName).Message);
    }

    private static EnumValueError CreateError<TEnum>(TEnum value, string propertyName) where TEnum : struct, Enum =>
        EnumMetadata.TryGet(out EnumInfo<TEnum>? info)
            ? info.CreateError(Text(value), EnumReadMode.Input, propertyName)
            : new EnumValueError(typeof(TEnum), Text(value), PlainEnum<TEnum>.Names, propertyName);

    private static string Text<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        if (EnumMetadata.TryGet(out EnumInfo<TEnum>? info))
            return info.IsDefined(value) ? info.Format(value, EnumWireFormat.String) : value.ToString("D");
        return PlainEnum<TEnum>.Contains(value) ? value.ToString() : value.ToString("D");
    }

    private static ulong ComputeMask<TEnum>(TEnum[] values) where TEnum : struct, Enum
    {
        ulong mask = 0;
        foreach (TEnum value in values)
            mask |= PlainEnum<TEnum>.ToRaw(value);
        return mask;
    }

    private static class PlainEnum<TEnum> where TEnum : struct, Enum
    {
#if NET5_0_OR_GREATER
        private static TEnum[] Values { get; } = Enum.GetValues<TEnum>();

        public static string[] Names { get; } = Enum.GetNames<TEnum>();
#else
        private static TEnum[] Values { get; } = (TEnum[])Enum.GetValues(typeof(TEnum));

        public static string[] Names { get; } = Enum.GetNames(typeof(TEnum));
#endif

        private static bool IsFlags { get; } = typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false);

        public static ulong Mask { get; } = ComputeMask<TEnum>(Values);

        public static bool Contains(TEnum value) =>
            Array.IndexOf(Values, value) >= 0 || IsFlags && (ToRaw(value) & ~Mask) == 0;

        public static ulong ToRaw(TEnum value) =>
            value.GetTypeCode() == TypeCode.UInt64
                ? Convert.ToUInt64(value, CultureInfo.InvariantCulture)
                : unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture));
    }
}
