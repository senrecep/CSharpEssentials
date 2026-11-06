using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.AspNetCore.ApiDocs.Benchmarks;

/// <summary>
/// The sample API every row describes: five Minimal API endpoints per resource, 50 in all, over 30 string enums.
/// Each resource has a list with an enum query, a get by id, an archive route with two trailing optional parameters,
/// an idempotent create and a delete with an enum query. The handlers never run during measurement.
/// </summary>
public static class SampleApi
{
    public const int EndpointCount = 50;
    public const int EnumCount = 30;
    public const int ResourceCount = 10;

    public static IEndpointRouteBuilder MapSampleApi(this IEndpointRouteBuilder app)
    {
        MapResource<Resource01, Enum01, Enum02>(app, "01");
        MapResource<Resource02, Enum04, Enum05>(app, "02");
        MapResource<Resource03, Enum07, Enum08>(app, "03");
        MapResource<Resource04, Enum10, Enum11>(app, "04");
        MapResource<Resource05, Enum13, Enum14>(app, "05");
        MapResource<Resource06, Enum16, Enum17>(app, "06");
        MapResource<Resource07, Enum19, Enum20>(app, "07");
        MapResource<Resource08, Enum22, Enum23>(app, "08");
        MapResource<Resource09, Enum25, Enum26>(app, "09");
        MapResource<Resource10, Enum28, Enum29>(app, "10");
        return app;
    }

    private static void MapResource<TResource, TStatus, TReason>(IEndpointRouteBuilder app, string k)
        where TResource : class
        where TStatus : struct, Enum
        where TReason : struct, Enum
    {
        string root = "/resources" + k;
        app.MapGet(root, static (TStatus status) => TypedResults.Ok(Array.Empty<TResource>()))
            .WithName("ListResources" + k);
        app.MapGet(root + "/{id:int}", static Results<Ok<TResource>, NotFound> (int id) => TypedResults.NotFound())
            .WithName("GetResource" + k);
        app.MapGet(root + "/archive/{year:int?}/{month:int?}", static (int? year, int? month) => TypedResults.Ok(Array.Empty<TResource>()))
            .WithName("GetArchive" + k);
        app.MapPost(root, static (TResource resource) => TypedResults.Created((string?)null, resource))
            .WithName("CreateResource" + k)
            .WithIdempotency();
        app.MapDelete(root + "/{id:int}", static (int id, TReason reason) => TypedResults.NoContent())
            .WithName("DeleteResource" + k);
    }
}
