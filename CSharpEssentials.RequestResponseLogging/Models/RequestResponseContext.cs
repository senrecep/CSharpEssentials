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


    private string? url;
    public string Url => url ??= BuildUrl().ToString();

    internal Uri BuildUrl()
    {
        string displayUrl = Context.Request.GetDisplayUrl();

        return new Uri(displayUrl, UriKind.RelativeOrAbsolute);
    }
}
