using Examples.Endpoints;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddAllServices();

var app = builder.Build();

app.MapAllEndpoints(options => options.OperationNaming = CSharpEssentials.Endpoints.OperationNaming.TypeName);

await app.RunAsync();
