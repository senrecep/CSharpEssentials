using Microsoft.Extensions.Logging;

namespace CSharpEssentials.DependencyInjection;

internal static partial class ServiceRegistrationLog
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "Service {ServiceType} with key {ServiceKey} is already registered by {ExistingImplementation}; applying {NewImplementation} with strategy {Strategy}")]
    public static partial void DuplicateRegistration(
        ILogger logger,
        Type serviceType,
        string serviceKey,
        string existingImplementation,
        string newImplementation,
        RegistrationStrategy strategy);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Warning,
        Message = "Some types of assembly {Assembly} could not be loaded and are skipped: {LoaderErrors}")]
    public static partial void TypesNotLoaded(ILogger logger, string assembly, string loaderErrors);
}
