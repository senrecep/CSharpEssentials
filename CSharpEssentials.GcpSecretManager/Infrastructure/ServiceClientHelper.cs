using Google.Cloud.SecretManager.V1;
using Grpc.Core;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.GcpSecretManager.Infrastructure.Internal;

internal interface IServiceClientHelper
{
    SecretManagerServiceClient Create();
    SecretManagerServiceClient Create(string credentialsPath);
    SecretManagerServiceClient CreateWithRegion(string? region);
    SecretManagerServiceClient CreateWithRegion(string credentialsPath, string? region);
}

internal sealed class ServiceClientHelper : IServiceClientHelper
{
    private static readonly AsyncRetryPolicy<SecretManagerServiceClient> RetryPolicy = Policy<SecretManagerServiceClient>
        .Handle<RpcException>(ex => ex.StatusCode == StatusCode.ResourceExhausted ||
                                  ex.StatusCode == StatusCode.Unavailable)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public SecretManagerServiceClient Create()
        => RetryPolicy.ExecuteAsync(() => SecretManagerServiceClient.CreateAsync()).GetAwaiter().GetResult();

    public SecretManagerServiceClient Create(string credentialsPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(credentialsPath);

        var clientBuilder = new SecretManagerServiceClientBuilder
        {
            CredentialsPath = credentialsPath
        };

        return RetryPolicy.ExecuteAsync(() => clientBuilder.BuildAsync()).GetAwaiter().GetResult();
    }

    public SecretManagerServiceClient CreateWithRegion(string? region)
    {
        var builder = new SecretManagerServiceClientBuilder();

        if (!string.IsNullOrEmpty(region))
        {
            builder.Endpoint = $"secretmanager.{region}.rep.googleapis.com";
        }

        return RetryPolicy.ExecuteAsync(() => builder.BuildAsync()).GetAwaiter().GetResult();
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

        return RetryPolicy.ExecuteAsync(() => clientBuilder.BuildAsync()).GetAwaiter().GetResult();
    }
}