using CSharpEssentials.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Tests.Fixtures.EndpointsA;

public sealed class FixtureAGroup : IEndpointGroup
{
    public static string Prefix => "fixture-a";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("FixtureA");
}
