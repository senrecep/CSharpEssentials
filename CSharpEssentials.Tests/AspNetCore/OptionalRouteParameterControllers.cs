using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore;

/// <summary>
/// Controllers with optional route parameters. Internal so the default <c>ControllerFeatureProvider</c> never discovers
/// them in other test hosts.
/// </summary>
internal static class OptionalRouteParameterControllers
{
    [ApiController]
    [Route("api/keys")]
    internal sealed class KeysController : ControllerBase
    {
        [HttpPost("{key?}", Name = "PostKey")]
        public string Post(string? key) => key ?? string.Empty;
    }

    [ApiController]
    internal sealed class RoutedController : ControllerBase
    {
        [HttpGet]
        [Route("api/routed/{id?}", Name = "GetRouted")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    [ApiController]
    [Route("api/{tenant?}/[controller]")]
    internal sealed class TenantsController : ControllerBase
    {
        [HttpGet(Name = "ListTenantItems")]
        public string List(string? tenant) => tenant ?? string.Empty;
    }

    [ApiController]
    [Route("api/scoped/{tenant?}")]
    internal sealed class ScopedController : ControllerBase
    {
        [HttpGet(Name = "GetScoped")]
        public string Get(string? tenant) => tenant ?? string.Empty;
    }

    [ApiController]
    internal sealed class ConstrainedController : ControllerBase
    {
        [HttpGet("api/constrained/{id:int?}", Name = "GetConstrained")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    [ApiController]
    internal sealed class PagesController : ControllerBase
    {
        [HttpGet("api/pages/{page=1}", Name = "GetPage")]
        public string Get(int page) => page.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    [ApiController]
    internal sealed class ArchiveController : ControllerBase
    {
        [HttpGet("api/archive/{year?}/{month?}", Name = "GetArchive")]
        public string Get(int? year, int? month) => $"{year}-{month}";
    }

    [ApiController]
    internal sealed class DeepController : ControllerBase
    {
        [HttpGet("api/deep/{a?}/{b?}/{c?}/{d?}", Name = "GetDeep")]
        public string Get(string? a, string? b, string? c, string? d) => $"{a}{b}{c}{d}";
    }

    [ApiController]
    internal sealed class FilesController : ControllerBase
    {
        [HttpGet("api/files/{**path}", Name = "GetFile")]
        public string Get(string? path) => path ?? string.Empty;
    }

    [ApiController]
    internal sealed class OrdersController : ControllerBase
    {
        [HttpGet("api/orders/{id?}", Name = "GetOrder")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        [HttpGet("api/orders", Name = "ListOrders")]
        public string List() => string.Empty;
    }

    [ApiController]
    internal sealed class AnonymousController : ControllerBase
    {
        [HttpGet("api/anonymous/{id?}")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    [ApiController]
    internal sealed class ClashController : ControllerBase
    {
        [HttpGet("api/clash/{id?}", Name = "GetClash")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        [HttpPost("api/clash-other", Name = "GetClashWithoutId")]
        public string Other() => string.Empty;
    }

    [ApiController]
    internal sealed class TripleController : ControllerBase
    {
        [HttpGet("api/triple/{a?}/{b?}/{c?}", Name = "GetTriple")]
        public string Get(string? a, string? b, string? c) => $"{a}{b}{c}";
    }

    [ApiController]
    internal sealed class RootController : ControllerBase
    {
        [HttpGet("{slug?}", Name = "GetRoot")]
        public string Get(string? slug) => slug ?? string.Empty;
    }

    [ApiController]
    [Route("api/notes/{id?}")]
    internal sealed class NotesController : ControllerBase
    {
        [HttpGet(Name = "GetNote")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        [HttpPost(Name = "PostNote")]
        public string Post(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    [ApiController]
    internal sealed class DownloadsController : ControllerBase
    {
        [HttpGet("api/downloads/{name}.{ext?}", Name = "GetDownload")]
        public string Get(string name, string? ext) => name + ext;
    }

    [ApiController]
    internal sealed class ItemsController : ControllerBase
    {
        [HttpPost("api/items", Name = "CreateItem")]
        public string Create() => string.Empty;

        [HttpGet("api/items/{id?}", Name = "GetItem")]
        public string Get(int? id) => id?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>The controllers of the 6.0.0 legacy capture (<c>swashbuckle-optional-route-legacy.json</c>).</summary>
    internal static readonly Type[] Legacy =
    [
        typeof(KeysController),
        typeof(RoutedController),
        typeof(TenantsController),
        typeof(ScopedController),
        typeof(ConstrainedController),
        typeof(PagesController),
        typeof(ArchiveController),
        typeof(DeepController),
        typeof(FilesController),
        typeof(OrdersController),
        typeof(AnonymousController),
    ];

    internal static readonly Type[] Compliant =
    [
        .. Legacy,
        typeof(TripleController),
        typeof(RootController),
        typeof(NotesController),
        typeof(DownloadsController),
        typeof(ItemsController),
    ];
}
