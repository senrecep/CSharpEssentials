using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>Internal so the default controller discovery of other hosts never finds it; added by <see cref="SampleApi.AddSampleControllers"/>.</summary>
[ApiController]
[Route("mvc")]
[ApiExplorerSettings(GroupName = "mvc")]
internal sealed class SampleOrdersController : ControllerBase
{
    [HttpGet("orders/{status}")]
    public SampleOrder Get(SampleStatus status, [FromQuery] SampleStatus? previous, [FromQuery] SampleStatus[] history) =>
        SampleHandlers.GetOrder(status, previous, history, SamplePermissions.None, SampleNaming.Original, SamplePlain.First);

    [HttpPost("orders")]
    public SampleOrder Create(SampleOrder order) => order;

    [HttpGet("legacy/{status}")]
    [EnumWireFormat(EnumWireFormat.Number)]
    public SampleOrder Legacy(SampleStatus status) =>
        SampleHandlers.GetOrder(status, null, [], SamplePermissions.None, SampleNaming.Original, SamplePlain.First);
}
