using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

/// <summary>
/// MVC controllers used only by the enum binding tests. They are nested (not public top-level types) so the
/// default <c>ControllerFeatureProvider</c> never discovers them in other test hosts; the enum binding hosts
/// register them explicitly through <see cref="EnumBindingHost"/>.
/// </summary>
public static class EnumBindingControllers
{
    [ApiController]
    [Route("")]
    public sealed class EbApiController : ControllerBase
    {
        [HttpGet("status")]
        public string Status(EbStatus status) => EbEcho.Of<EbStatus>(status);

        [HttpGet("items/{status}")]
        public string Item(EbStatus status) => EbEcho.Of<EbStatus>(status);

        [HttpGet("nullable")]
        public string Nullable(EbStatus? status) => EbEcho.Of(status);

        [HttpGet("array")]
        public string Array([FromQuery] List<EbStatus> s) => EbEcho.Of<EbStatus>(s);

        [HttpGet("flags")]
        public string Flags(EbPermission perms) => EbEcho.Of<EbPermission>(perms);

        [HttpGet("renamed")]
        public string Renamed([FromQuery(Name = "st")] EbStatus status) => EbEcho.Of<EbStatus>(status);

        [HttpGet("custom")]
        public string Custom(EbCustom value) => EbEcho.Of<EbCustom>(value);

        [HttpGet("plain")]
        public string Plain(EbPlain plain) => plain.ToString();

        [HttpGet("multi")]
        public string Multi(EbStatus a, EbPermission b) => $"{a}|{b}";

        [HttpGet("none")]
        public string None(string? x) => $"ok:{x}";

        [HttpGet("dto")]
        public string Dto([FromQuery] EbQueryDto dto) => $"{dto.Status}|{EbEcho.Of(dto.Optional)}";
    }

    [Route("")]
    public sealed class EbMvcController : ControllerBase
    {
        private string Echo(string value) => ModelState.IsValid ? value : "invalid";

        [HttpGet("status")]
        public string Status(EbStatus status) => Echo(EbEcho.Of<EbStatus>(status));

        [HttpGet("items/{status}")]
        public string Item(EbStatus status) => Echo(EbEcho.Of<EbStatus>(status));

        [HttpGet("nullable")]
        public string Nullable(EbStatus? status) => Echo(EbEcho.Of(status));

        [HttpGet("array")]
        public string Array([FromQuery] List<EbStatus> s) => Echo(EbEcho.Of<EbStatus>(s));

        [HttpGet("flags")]
        public string Flags(EbPermission perms) => Echo(EbEcho.Of<EbPermission>(perms));

        [HttpGet("renamed")]
        public string Renamed([FromQuery(Name = "st")] EbStatus status) => Echo(EbEcho.Of<EbStatus>(status));

        [HttpGet("custom")]
        public string Custom(EbCustom value) => Echo(EbEcho.Of<EbCustom>(value));

        [HttpGet("plain")]
        public string Plain(EbPlain plain) => Echo(plain.ToString());

        [HttpGet("multi")]
        public string Multi(EbStatus a, EbPermission b) => Echo($"{a}|{b}");

        [HttpGet("none")]
        public string None(string? x) => Echo($"ok:{x}");

        [HttpGet("dto")]
        public string Dto([FromQuery] EbQueryDto dto) => Echo($"{dto.Status}|{EbEcho.Of(dto.Optional)}");
    }
}
