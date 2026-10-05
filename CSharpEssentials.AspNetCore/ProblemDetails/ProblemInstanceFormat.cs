namespace CSharpEssentials.AspNetCore;

/// <summary>
/// How <c>ProblemDetails.Instance</c> is filled when it is not set explicitly.
/// </summary>
public enum ProblemInstanceFormat
{
    /// <summary>
    /// The request path, e.g. <c>/users/42</c>.
    /// </summary>
    Path = 0,

    /// <summary>
    /// The request method and path, e.g. <c>GET /users/42</c> (3.x behavior).
    /// </summary>
    MethodAndPath = 1,

    /// <summary>
    /// <c>Instance</c> is left empty.
    /// </summary>
    None = 2,
}
