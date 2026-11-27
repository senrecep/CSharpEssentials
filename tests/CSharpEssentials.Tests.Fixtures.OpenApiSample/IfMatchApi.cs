using System.Reflection;
using CSharpEssentials.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The <c>If-Match</c> endpoints both OpenAPI packages describe: Minimal API endpoints with an optional and a required
/// header, one with its own <c>412</c>, a <c>GET</c> in an <c>If-Match</c> group, and <see cref="IfMatchController"/>. One
/// document, <see cref="Document"/>.
/// </summary>
public static class IfMatchApi
{
    public const string Document = "if-match";

    public static IEndpointRouteBuilder MapIfMatchApi(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/if-match").WithGroupName(Document).WithIfMatch();
        group.MapPut("/optional", static () => 1);
        group.MapPut("/required", static () => 1).WithIfMatch(required: true);
        group.MapPut("/own-412", static () => 1).Produces<string>(StatusCodes.Status412PreconditionFailed, "text/plain");
        group.MapGet("/read", static () => 1);
        return app;
    }

    /// <summary>Adds <see cref="IfMatchController"/> only (it is internal, so no other host discovers it).</summary>
    public static IMvcBuilder AddIfMatchControllers(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureApplicationPartManager(static manager =>
            manager.FeatureProviders.Add(new IfMatchControllerFeatureProvider()));
    }

    private sealed class IfMatchControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo controller = typeof(IfMatchController).GetTypeInfo();
            if (!feature.Controllers.Contains(controller))
                feature.Controllers.Add(controller);
        }
    }
}
