using System.Reflection;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>
/// The idempotent API both OpenAPI packages describe in the golden tests: Minimal API endpoints with a required key, an
/// optional key, a profile, an own <c>409</c>, a method that takes no part, the modes <c>Off</c> and <c>Observe</c>, and no
/// idempotency, plus <see cref="IdempotencyController"/>. One document, <see cref="Document"/>.
/// </summary>
public static class IdempotencyApi
{
    public const string Document = "idempotency";

    public const string StrictProfile = "strict";

    /// <summary>Registers the <see cref="StrictProfile"/> profile (key required, unscoped keys rejected).</summary>
    public static void ConfigureOptions(IdempotencyOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.AddProfile(StrictProfile, static profile =>
        {
            profile.RequireKey = true;
            profile.UnscopedKeyBehavior = UnscopedKeyBehavior.Reject;
        });
    }

    public static IEndpointRouteBuilder MapIdempotencyApi(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RouteGroupBuilder group = app.MapGroup("/idempotency").WithGroupName(Document);
        group.MapPost("/required", static () => 1).WithIdempotency(static options => options.RequireKey = true);
        group.MapPost("/optional", static () => 1).WithIdempotency();
        group.MapPost("/profile", static () => 1).WithIdempotency(StrictProfile);
        group.MapPost("/business-conflict", static () => 1).WithIdempotency().ProducesProblem(StatusCodes.Status409Conflict);
        group.MapGet("/get", static () => 1).WithIdempotency();
        group.MapPost("/off", static () => 1).WithIdempotency(static options => options.Mode = IdempotencyMode.Off);
        group.MapPost("/observe", static () => 1).WithIdempotency(static options => options.Mode = IdempotencyMode.Observe);
        group.MapPost("/plain", static () => 1);
        return app;
    }

    /// <summary>Adds <see cref="IdempotencyController"/> only (it is internal, so no other host discovers it).</summary>
    public static IMvcBuilder AddIdempotencyControllers(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ConfigureApplicationPartManager(static manager =>
            manager.FeatureProviders.Add(new IdempotencyControllerFeatureProvider()));
    }

    private sealed class IdempotencyControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo controller = typeof(IdempotencyController).GetTypeInfo();
            if (!feature.Controllers.Contains(controller))
                feature.Controllers.Add(controller);
        }
    }
}
