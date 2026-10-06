using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>
/// Internal controllers, registered explicitly by <see cref="EnumConventionsHost"/> so other test hosts never discover them.
/// </summary>
internal static class EnumConventionsControllers
{
    /// <summary>The MVC half of the binding matrix; <c>/min/...</c> maps the same routes as Minimal API endpoints.</summary>
    [ApiController]
    [Route("mvc")]
    internal sealed class EcMatrixController : ControllerBase
    {
        [HttpGet("route/{status}")]
        public string Route(EcStatus status) => EcEcho.Of<EcStatus>(status);

        [HttpGet("route-flags/{perms}")]
        public string RouteFlags(EcPermission perms) => EcEcho.Of<EcPermission>(perms);

        [HttpGet("query")]
        public string Query([FromQuery] EcStatus status) => EcEcho.Of<EcStatus>(status);

        [HttpGet("query-nullable")]
        public string QueryNullable([FromQuery] EcStatus? status) => EcEcho.Of(status);

        [HttpGet("query-array")]
        public string QueryArray([FromQuery] EcStatus[] status) => EcEcho.Of<EcStatus>(status);

        [HttpGet("query-flags")]
        public string QueryFlags([FromQuery] EcPermission perms) => EcEcho.Of<EcPermission>(perms);

        [HttpGet("header")]
        public string Header([FromHeader(Name = "X-Status")] EcStatus status) => EcEcho.Of<EcStatus>(status);

        [HttpGet("header-nullable")]
        public string HeaderNullable([FromHeader(Name = "X-Status")] EcStatus? status) => EcEcho.Of(status);

        [HttpGet("header-array")]
        public string HeaderArray([FromHeader(Name = "X-Status")] EcStatus[] status) => EcEcho.Of<EcStatus>(status);

        [HttpGet("header-flags")]
        public string HeaderFlags([FromHeader(Name = "X-Perms")] EcPermission perms) => EcEcho.Of<EcPermission>(perms);

        [HttpPost("form")]
        public string Form([FromForm] EcStatus status) => EcEcho.Of<EcStatus>(status);

        [HttpPost("form-nullable")]
        public string FormNullable([FromForm] EcStatus? status) => EcEcho.Of(status);

        [HttpPost("form-array")]
        public string FormArray([FromForm] EcStatus[] status) => EcEcho.Of<EcStatus>(status);

        [HttpPost("form-flags")]
        public string FormFlags([FromForm] EcPermission perms) => EcEcho.Of<EcPermission>(perms);

        [HttpGet("plain")]
        public string Plain(EcPlain plain) => plain.ToString();

        [HttpGet("plain-order")]
        public EcPlainOrder PlainOrder() => new(1, EcPlain.InProgress);

        [HttpPost("body")]
        public string Body(EcStatusBody body) => EcEcho.Of<EcStatus>(body.Status);
    }

    /// <summary>Controller attribute <see cref="EnumWireFormat.String"/>; actions override it.</summary>
    [ApiController]
    [Route("wire/string-controller")]
    [EnumWireFormat(EnumWireFormat.String)]
    internal sealed class EcStringController : ControllerBase
    {
        [HttpGet("inherit")]
        public EcOrder Inherit() => EcEcho.Order;

        [HttpGet("action-number")]
        [EnumWireFormat(EnumWireFormat.Number)]
        public EcOrder ActionNumber() => EcEcho.Order;

        [HttpGet("json-result")]
        [EnumWireFormat(EnumWireFormat.Number)]
        public JsonResult JsonResult() => new(EcEcho.Order);
    }

    /// <summary>Controller attribute <see cref="EnumWireFormat.Number"/>; actions override it.</summary>
    [ApiController]
    [Route("wire/number-controller")]
    [EnumWireFormat(EnumWireFormat.Number)]
    internal sealed class EcNumberController : ControllerBase
    {
        [HttpGet("inherit")]
        public EcOrder Inherit() => EcEcho.Order;

        [HttpGet("action-string")]
        [EnumWireFormat(EnumWireFormat.String)]
        public EcOrder ActionString() => EcEcho.Order;
    }

    /// <summary>No attribute: the endpoint convention (group) or the global format applies.</summary>
    [ApiController]
    [Route("wire/plain-controller")]
    internal sealed class EcNoAttributeController : ControllerBase
    {
        [HttpGet("inherit")]
        public EcOrder Inherit() => EcEcho.Order;

        [HttpGet("action-number")]
        [EnumWireFormat(EnumWireFormat.Number)]
        public EcOrder ActionNumber() => EcEcho.Order;

        [HttpGet("action-string")]
        [EnumWireFormat(EnumWireFormat.String)]
        public EcOrder ActionString() => EcEcho.Order;

        [HttpGet("created")]
        [EnumWireFormat(EnumWireFormat.Number)]
        public CreatedResult CreatedOrder() => Created("/orders/1", EcEcho.Order);
    }
}
