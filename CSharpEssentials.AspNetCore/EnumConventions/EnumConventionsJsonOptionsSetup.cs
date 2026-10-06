using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Adds the enum converter (<see cref="EnumReadMode.Input"/>, <see cref="EnumConventions.WriteAs"/>) to the minimal API and MVC
/// JSON options. Post-configuration, so it wins over converters added by other <c>Configure</c> calls.
/// </summary>
internal sealed class EnumConventionsJsonOptionsSetup(EnumConventionsRegistration registration)
    : IPostConfigureOptions<HttpJsonOptions>, IPostConfigureOptions<MvcJsonOptions>
{
    public void PostConfigure(string? name, HttpJsonOptions options) => Apply(options.SerializerOptions, registration.Conventions.WriteAs);

    public void PostConfigure(string? name, MvcJsonOptions options) => Apply(options.JsonSerializerOptions, registration.Conventions.WriteAs);

    private void Apply(JsonSerializerOptions options, EnumWireFormat writeAs) =>
        UseFactory(options, registration.CreateConverterFactory(writeAs));

    // AddEnumConventions removes earlier convention factories and inserts its factory at position 0 (other converters stay); that slot then takes the
    // registration's factory, which keeps the reflection opt-in.
    internal static void UseFactory(JsonSerializerOptions options, EnumConverterFactory factory)
    {
        options.AddEnumConventions(factory.Conventions, factory.Mode, factory.WriteAs);
        options.Converters[0] = factory;
    }
}
