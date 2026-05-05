using System.Text.Json.Serialization;

namespace CSharpEssentials.RequestResponseLogging;

public class RequestResponseContext
{
    [JsonIgnore]
    internal readonly HttpContext Context;

    internal RequestResponseContext(HttpContext context) => Context = context;

    public string? RequestBody { get; internal set; }

    public string? ResponseBody { get; internal set; }

    [JsonIgnore]
    public TimeSpan? ResponseCreationTime { get; internal set; }

    /// <summary>
    /// Gets total response duration. Format: mm:ss.fff
    /// </summary>
    public string ResponseTime => ResponseCreationTime is null ? string.Empty : $"{ResponseCreationTime:mm\\:ss\\.fff}";


    public int? RequestLength => RequestBody?.Length == 0 ? null : RequestBody?.Length;

    public int? ResponseLength => ResponseBody?.Length;


#pragma warning disable IDE0032 // Auto-property not possible due to BuildUrl() call
    private string? _url;
    public string Url => _url ??= BuildUrl().ToString();
#pragma warning restore IDE0032

    internal Uri BuildUrl()
    {
        string displayUrl = Context.Request.GetDisplayUrl();

        return new Uri(displayUrl, UriKind.RelativeOrAbsolute);
    }
}
