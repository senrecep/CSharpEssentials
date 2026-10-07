using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CSharpEssentials.AspNetCore;

public sealed class DefaultConfigureSwaggerOptions(
           IServiceProvider serviceProvider,
           IHostEnvironment environment,
           IConfiguration configuration)
           : ConfigureSwaggerOptions(serviceProvider, environment, configuration);
