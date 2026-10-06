using System.Text.Json;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The JSON options of the output format that is not <see cref="EnumConventions.WriteAs"/>. Each one is built once, on first use,
/// by the options factory: the same configuration as the host's JSON options with the other enum format. The host's
/// options are never changed.
/// </summary>
internal sealed class EnumWireFormatOutput
{
    private readonly Lazy<IOptions<HttpJsonOptions>> _httpOptions;
    private readonly Lazy<JsonSerializerOptions> _mvcOptions;
    private readonly Lazy<SystemTextJsonOutputFormatter> _mvcFormatter;

    public EnumWireFormatOutput(
        EnumConventionsRegistration registration,
        IOptionsFactory<HttpJsonOptions> httpOptionsFactory,
        IOptionsFactory<MvcJsonOptions> mvcOptionsFactory)
    {
        DefaultFormat = registration.Conventions.WriteAs;
        EnumWireFormat other = DefaultFormat == EnumWireFormat.String ? EnumWireFormat.Number : EnumWireFormat.String;

        _httpOptions = new(() =>
        {
            HttpJsonOptions options = httpOptionsFactory.Create(Options.DefaultName);
            EnumConventionsJsonOptionsSetup.UseFactory(options.SerializerOptions, registration.CreateConverterFactory(other));
            return Options.Create(options);
        });
        _mvcOptions = new(() =>
        {
            MvcJsonOptions options = mvcOptionsFactory.Create(Options.DefaultName);
            EnumConventionsJsonOptionsSetup.UseFactory(options.JsonSerializerOptions, registration.CreateConverterFactory(other));
            return options.JsonSerializerOptions;
        });
        _mvcFormatter = new(() => new SystemTextJsonOutputFormatter(_mvcOptions.Value));
    }

    /// <summary><see cref="EnumConventions.WriteAs"/>: the format of the host's own JSON options.</summary>
    public EnumWireFormat DefaultFormat { get; }

    /// <summary>Minimal API JSON options with the other format.</summary>
    public IOptions<HttpJsonOptions> HttpOptions => _httpOptions.Value;

    /// <summary>MVC JSON serializer options with the other format.</summary>
    public JsonSerializerOptions MvcOptions => _mvcOptions.Value;

    /// <summary>An MVC output formatter with <see cref="MvcOptions"/>.</summary>
    public SystemTextJsonOutputFormatter MvcFormatter => _mvcFormatter.Value;
}
