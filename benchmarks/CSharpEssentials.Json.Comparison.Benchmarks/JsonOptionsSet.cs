using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>The enum options each library documents, built once from the same web defaults.</summary>
internal static class JsonOptionsSet
{
    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    public static readonly JsonSerializerOptions CseData =
        new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Json/Readme.MD#enum-conventions
    public static readonly JsonSerializerOptions CseInput =
        new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);

    // https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/customize-properties#enums-as-strings
    public static readonly JsonSerializerOptions Bcl =
        new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) } };

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    public static readonly JsonSerializerOptions Macross =
        new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumMemberConverter(JsonNamingPolicy.SnakeCaseLower) } };

    // https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations
    public static readonly JsonSerializerOptions MacrossFallback =
        new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumMemberConverter(
                    new JsonStringEnumMemberConverterOptions(JsonNamingPolicy.SnakeCaseLower, deserializationFailureFallbackValue: OrderStatus.Unknown)),
            },
        };
}
