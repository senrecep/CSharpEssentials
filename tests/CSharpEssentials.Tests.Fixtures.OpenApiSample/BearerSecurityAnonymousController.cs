using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>An anonymous controller with one authorized action, for the bearer security tests.</summary>
[ApiController]
[AllowAnonymous]
[Route("bearer/anonymous-controller")]
public sealed class BearerSecurityAnonymousController : ControllerBase
{
    /// <summary>Stays anonymous: the controller's <see cref="AllowAnonymousAttribute"/> wins.</summary>
    /// <returns><c>200 OK</c>.</returns>
    [HttpGet("authorized")]
    [Authorize]
    public IActionResult Authorized() => Ok();
}
