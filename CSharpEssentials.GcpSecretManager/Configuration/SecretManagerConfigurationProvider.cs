using System.Collections.Concurrent;
using System.Text.Json;
using CSharpEssentials.GcpSecretManager.Infrastructure;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using Google.Api.Gax;
using Google.Cloud.SecretManager.V1;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;

namespace CSharpEssentials.GcpSecretManager.Configuration;

internal sealed class SecretManagerConfigurationProvider(
    List<ProjectSecretLoadContext> projectConfigs,
    ISecretManagerConfigurationLoader loader,
    SecretManagerConfigurationOptions options
) : ConfigurationProvider
{
    private const char _separator = ':';

    private static readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<RpcException>(ex => ex.StatusCode == StatusCode.ResourceExhausted ||
                                  ex.StatusCode == StatusCode.Unavailable)
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    private readonly ConcurrentDictionary<string, string?> _data = new();

    public override void Load()
        => LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();

    public async Task LoadAsync()
    {
        try
        {
            var tasks = projectConfigs.Select(config =>
                Task.Run(() => LoadProjectSecretsAsync(config))).ToList();

            Dictionary<string, string?>[] results = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (Dictionary<string, string?> result in results)
            {
                foreach ((string key, string value) in result)
                {
                    _data.TryAdd(key, value);
                }
            }

            Data = new Dictionary<string, string?>(_data, StringComparer.Ordinal);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Critical error during secret loading: {ex}");
            throw;
        }
    }

    private async Task<Dictionary<string, string?>> LoadProjectSecretsAsync(ProjectSecretLoadContext context)
    {
        string parent = SecretManagerPaths.BuildParentPath(context.ProjectName.ProjectId, context.Config.Region);

        try
        {
            List<Secret> secrets = await _retryPolicy.ExecuteAsync(() =>
                ListSecretsAsync(context, parent)).ConfigureAwait(false);

            if (secrets.Count == 0)
                return [];

            var filteredSecrets = secrets
                .Where(secret => loader.ShouldLoadSecret(secret, context.Config))
                .ToList();

            if (filteredSecrets.Count == 0)
                return [];

            Dictionary<string, string?> resultDict = new(StringComparer.Ordinal);

            for (int i = 0; i < filteredSecrets.Count; i += options.BatchSize)
            {
                IEnumerable<Secret> batch = filteredSecrets.Skip(i).Take(options.BatchSize);
                IEnumerable<Task> loadTasks = batch.Select(secret =>
                    LoadSecretAndAddToDictionaryAsync(context, secret, resultDict));

                await Task.WhenAll(loadTasks).ConfigureAwait(false);
            }

            return resultDict;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error loading secrets for {parent}: {ex}");
            return new Dictionary<string, string?>(StringComparer.Ordinal);
        }
    }

    private async Task LoadSecretAndAddToDictionaryAsync(
        ProjectSecretLoadContext context,
        Secret secret,
        IDictionary<string, string?> resultDict)
    {
        try
        {
            string secretPath = SecretManagerPaths.BuildSecretPath(
                    context.ProjectName.ProjectId,
                    context.Config.Region,
                secret.SecretName.SecretId);

            Console.WriteLine($"Started loading secret: {secretPath}");

            SecretLoadResult result = await LoadSecretValueAsync(context, secret, secretPath).ConfigureAwait(false);
            string jsonValue = result.Value;

            resultDict.TryAdd(result.Key, jsonValue);

            if (!context.Config.IsRawSecret(secret.SecretName.SecretId))
                TryParseAndFlattenJson(resultDict, result, jsonValue);

            Console.WriteLine($"Completed loading secret: {secretPath}");

        }
        catch (RpcException ex)
        {
            await Console.Error.WriteLineAsync($"Failed to load secret {secret.SecretName.SecretId}: {ex.StatusCode}");
        }
    }

    private static void TryParseAndFlattenJson(IDictionary<string, string?> resultDict, SecretLoadResult result, string jsonValue)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonValue);
            var tempData = new Dictionary<string, string?>(StringComparer.Ordinal);
            FlattenJson(tempData, document.RootElement, result.Key);

            if (tempData.Count > 0)
                foreach ((string key, string value) in tempData)
                    resultDict.TryAdd(key, value);
        }
        catch
        {
            // If JSON parsing fails, we already have the raw value saved, so just continue
        }
    }

    private async Task<List<Secret>> ListSecretsAsync(ProjectSecretLoadContext context, string parent)
    {
        try
        {
            var request = new ListSecretsRequest
            {
                Parent = parent,
                PageSize = options.PageSize
            };

            PagedAsyncEnumerable<ListSecretsResponse, Secret> response = context.Client.ListSecretsAsync(request);
            var secrets = new List<Secret>();

            await foreach (Secret? secret in response.ConfigureAwait(false))
            {
                secrets.Add(secret);
            }

            return secrets;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error listing secrets: {ex.Message}");
            return [];
        }
    }

    private async Task<SecretLoadResult> LoadSecretValueAsync(
        ProjectSecretLoadContext context,
        Secret secret,
        string secretPath)
    {
        var request = new AccessSecretVersionRequest { Name = secretPath };
        AccessSecretVersionResponse response = await context.Client.AccessSecretVersionAsync(request).ConfigureAwait(false);

        return new SecretLoadResult(
            secretPath,
            response.Payload.Data.ToStringUtf8(),
            loader.GetKey(secret));
    }

    private static void FlattenJson(IDictionary<string, string?> data, JsonElement element, string parentPath)
    {

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string newPath = string.IsNullOrEmpty(parentPath)
                        ? property.Name
                        : string.Concat(parentPath, _separator, property.Name);
                    FlattenJson(data, property.Value, newPath);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                {
                    string newPath = string.Concat(parentPath, _separator, index++);
                    FlattenJson(data, item, newPath);
                }
                break;

            case JsonValueKind.Null:
                data[parentPath] = null;
                break;

            default:
                data[parentPath] = element.ToString();
                break;
        }
    }
}