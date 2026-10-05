using Google.Cloud.SecretManager.V1;

namespace CSharpEssentials.GcpSecretManager.Infrastructure;

internal interface IServiceClientHelper
{
    SecretManagerServiceClient Create();
    SecretManagerServiceClient Create(string credentialsPath);
    SecretManagerServiceClient CreateWithRegion(string? region);
    SecretManagerServiceClient CreateWithRegion(string credentialsPath, string? region);
}

internal sealed class ServiceClientHelper : IServiceClientHelper
{
    public SecretManagerServiceClient Create()
        => SecretManagerServiceClient.Create();

    public SecretManagerServiceClient Create(string credentialsPath)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(credentialsPath);
#else
        if (string.IsNullOrEmpty(credentialsPath))
            throw new ArgumentException("Value cannot be null or empty.", nameof(credentialsPath));
#endif

        var clientBuilder = new SecretManagerServiceClientBuilder
        {
            CredentialsPath = credentialsPath
        };

        return clientBuilder.Build();
    }

    public SecretManagerServiceClient CreateWithRegion(string? region)
    {
        var builder = new SecretManagerServiceClientBuilder();

        if (!string.IsNullOrEmpty(region))
        {
            builder.Endpoint = $"secretmanager.{region}.rep.googleapis.com";
        }

        return builder.Build();
    }

    public SecretManagerServiceClient CreateWithRegion(string credentialsPath, string? region)
    {
        var clientBuilder = new SecretManagerServiceClientBuilder
        {
            CredentialsPath = credentialsPath
        };

        if (!string.IsNullOrEmpty(region))
        {
            clientBuilder.Endpoint = $"secretmanager.{region}.rep.googleapis.com";
        }

        return clientBuilder.Build();
    }
}
