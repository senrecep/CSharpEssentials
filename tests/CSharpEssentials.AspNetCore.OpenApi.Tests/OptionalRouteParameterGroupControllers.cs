using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

/// <summary>
/// Controllers with optional route parameters in the API explorer groups <c>v1</c> and <c>v2</c>. Internal so the default
/// <c>ControllerFeatureProvider</c> never discovers them in other test hosts.
/// </summary>
internal static class OptionalRouteParameterGroupControllers
{
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    internal sealed class SharedV1Controller : ControllerBase
    {
        [HttpGet("api/shared/{id?}", Name = "GetSharedV1")]
        public string Get(string? id) => id ?? string.Empty;
    }

    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    internal sealed class SharedV2Controller : ControllerBase
    {
        [HttpGet("api/shared/{id?}", Name = "GetSharedV2")]
        public string Get(string? id) => id ?? string.Empty;
    }

    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    internal sealed class ReportsController : ControllerBase
    {
        [HttpGet("api/reports/{year?}", Name = "GetReport")]
        public string Get(int? year) => year?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }
}
