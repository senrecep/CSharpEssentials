using CSharpEssentials.Endpoints;
using Examples.Endpoints;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddAuthentication().AddBearerToken();
builder.Services.AddAuthorization();
builder.Services.AddAllServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.GetEndpoint()?.Metadata.GetMetadata<EndpointTypeMetadata>() is { } metadata)
    {
        context.Response.Headers["X-Endpoint-Type"] = metadata.EndpointType.Name;
    }

    await next(context);
});

app.MapAllEndpoints(options =>
{
    options.OperationNaming = OperationNaming.TypeName;
    options.AutoTagFromGroup = true;
    options.ConfigureEach((endpoint, type) => endpoint.WithDescription($"Mapped from {type.Name}"));
});

await app.RunAsync();
