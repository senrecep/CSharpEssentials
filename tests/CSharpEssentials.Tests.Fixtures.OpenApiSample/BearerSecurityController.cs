using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>An authorized controller with one anonymous action, for the bearer security tests.</summary>
[ApiController]
[Authorize]
[Route("bearer/controller")]
public sealed class BearerSecurityController : ControllerBase
{
    /// <summary>Requires authorization.</summary>
    /// <returns><c>200 OK</c>.</returns>
    [HttpGet("secure")]
    public IActionResult Secure() => Ok();

    /// <summary>Allows anonymous access.</summary>
    /// <returns><c>200 OK</c>.</returns>
    [HttpGet("anonymous")]
    [AllowAnonymous]
    public IActionResult Anonymous() => Ok();
}
