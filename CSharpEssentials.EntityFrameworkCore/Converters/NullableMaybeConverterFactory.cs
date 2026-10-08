using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Creates the converter for a <c>Maybe&lt;T&gt;?</c> property found by <see cref="NullableMaybeConvention"/>.
/// </summary>
/// <remarks>
/// The convention only knows <c>T</c> as a <see cref="Type"/>, so the converter type is closed with
/// <see cref="Type.MakeGenericType"/>. That happens once per <c>T</c>, while the model is built, and the converter is cached;
/// reads and writes run the converter's compiled expressions without reflection.
/// </remarks>
internal static class NullableMaybeConverterFactory
{
    private static readonly ConcurrentDictionary<Type, ValueConverter> Converters = new();

    internal static ValueConverter Create(Type valueType) =>
        Converters.GetOrAdd(valueType, static type =>
        {
            Type converterType = type.IsValueType
                ? typeof(NullableMaybeConverter<>)
                : typeof(NullableMaybeReferenceConverter<>);
            return (ValueConverter)Activator.CreateInstance(converterType.MakeGenericType(type))!;
        });
}
