using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.AspNetCore;

public static partial class Extensions
{
    public static ApiVersionSet CreateVersionSet(
        this IEndpointRouteBuilder app,
        int version = 1)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(version))
            .ReportApiVersions()
            .Build();
    }

    public static RouteGroupBuilder CreateVersionedGroup(
        this IEndpointRouteBuilder app,
        string route,
        int version = 1)
    {
        ApiVersionSet apiVersionSet = app.CreateVersionSet(version);

        RouteGroupBuilder versionedGroup = app
            .MapGroup($"v{{version:apiVersion}}/{route}")
            .WithApiVersionSet(apiVersionSet);

        return versionedGroup;
    }

    /// <summary>
    /// Creates a route group with the <c>v{version:apiVersion}</c> prefix and an API version set for <paramref name="version"/>.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <param name="version">The major API version of the group.</param>
    /// <returns>The versioned route group.</returns>
    public static RouteGroupBuilder MapVersionedGroup(
        this IEndpointRouteBuilder app,
        int version)
    {
        return app
            .MapGroup("v{version:apiVersion}")
            .WithApiVersionSet(app.CreateVersionSet(version));
    }
}
