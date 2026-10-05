namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Built-in resolvers for <c>ProblemDetails.Type</c>.
/// </summary>
public static class ProblemTypeUris
{
    /// <summary>
    /// RFC 9110 section links, matching the ones ASP.NET Core writes for framework responses.
    /// Returns <see langword="null"/> for status codes RFC 9110 does not define.
    /// </summary>
    public static string? Rfc9110(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        405 => "https://tools.ietf.org/html/rfc9110#section-15.5.6",
        406 => "https://tools.ietf.org/html/rfc9110#section-15.5.7",
        408 => "https://tools.ietf.org/html/rfc9110#section-15.5.9",
        409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
        410 => "https://tools.ietf.org/html/rfc9110#section-15.5.11",
        411 => "https://tools.ietf.org/html/rfc9110#section-15.5.12",
        412 => "https://tools.ietf.org/html/rfc9110#section-15.5.13",
        413 => "https://tools.ietf.org/html/rfc9110#section-15.5.14",
        414 => "https://tools.ietf.org/html/rfc9110#section-15.5.15",
        415 => "https://tools.ietf.org/html/rfc9110#section-15.5.16",
        416 => "https://tools.ietf.org/html/rfc9110#section-15.5.17",
        417 => "https://tools.ietf.org/html/rfc9110#section-15.5.18",
        422 => "https://tools.ietf.org/html/rfc9110#section-15.5.21",
        426 => "https://tools.ietf.org/html/rfc9110#section-15.5.22",
        500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        501 => "https://tools.ietf.org/html/rfc9110#section-15.6.2",
        502 => "https://tools.ietf.org/html/rfc9110#section-15.6.3",
        503 => "https://tools.ietf.org/html/rfc9110#section-15.6.4",
        504 => "https://tools.ietf.org/html/rfc9110#section-15.6.5",
        505 => "https://tools.ietf.org/html/rfc9110#section-15.6.6",
        _ => null,
    };

    /// <summary>
    /// The RFC 7231/7235 links written by 3.x. Unknown status codes map to the 500 link.
    /// </summary>
    public static string? Rfc7231(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        405 => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };
}
