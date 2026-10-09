using CSharpEssentials.RequestResponseLogging;
using Examples.RequestResponseLogging.Infrastructure;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.AddRequestResponseLogging(opt =>
{
    opt.IgnorePaths("/health");
    var loggingOptions = LoggingOptions.CreateAllFields();
    loggingOptions.HeaderKeys.Add(HeaderNames.AcceptLanguage);
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions);
});
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
