using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using CSharpEssentials.GcpSecretManager.Infrastructure;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using CSharpEssentials.Resilience;
using CSharpEssentials.ResultPattern;
using Google.Api.Gax;
using Google.Cloud.SecretManager.V1;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpEssentials.GcpSecretManager.Configuration;

internal sealed partial class SecretManagerConfigurationProvider(
    List<ProjectSecretLoadContext> projectConfigs,
    ISecretManagerConfigurationLoader loader,
    SecretManagerConfigurationOptions options,
    TimeSpan? retryBaseDelay = null
) : ConfigurationProvider
{
    private const char _separator = ':';

    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan DefaultRetryBaseDelay = TimeSpan.FromSeconds(2);

    private readonly TimeSpan _retryBaseDelay = retryBaseDelay ?? DefaultRetryBaseDelay;
    private readonly ConcurrentDictionary<string, string?> _data = new();
    private readonly ILogger _logger = options.LoggerFactory?.CreateLogger<SecretManagerConfigurationProvider>()
        ?? NullLogger<SecretManagerConfigurationProvider>.Instance;

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
            LogLoadFailed(ex);
            throw;
        }
    }

    private async Task<Dictionary<string, string?>> LoadProjectSecretsAsync(ProjectSecretLoadContext context)
    {
        string parent = SecretManagerPaths.BuildParentPath(context.ProjectName.ProjectId, context.Config.Region);

        try
        {
            List<Secret> secrets = await ExecuteWithRetryAsync(
                () => ListSecretsAsync(context, parent)).ConfigureAwait(false);

            if (secrets.Count == 0)
                return [];

            var filteredSecrets = secrets
                .Where(secret => loader.ShouldLoadSecret(secret, context.Config))
                .ToList();

            if (filteredSecrets.Count == 0)
                return [];

            var resultDict = new Dictionary<string, string?>(StringComparer.Ordinal);

            for (int i = 0; i < filteredSecrets.Count; i += options.BatchSize)
            {
                IEnumerable<Secret> batch = filteredSecrets.Skip(i).Take(options.BatchSize);
                IEnumerable<Task<Dictionary<string, string?>>> loadTasks = batch.Select(secret =>
                    LoadSecretAsync(context, secret));

                Dictionary<string, string?>[] batchResults = await Task.WhenAll(loadTasks).ConfigureAwait(false);

                foreach (Dictionary<string, string?> secretValues in batchResults)
                {
                    foreach ((string key, string? value) in secretValues)
                    {
                        resultDict.TryAdd(key, value);
                    }
                }
            }

            return resultDict;
        }
        catch (Exception ex)
        {
            LogProjectLoadFailed(ex, parent);
            return [];
        }
    }

    private async Task<Dictionary<string, string?>> LoadSecretAsync(
        ProjectSecretLoadContext context,
        Secret secret)
    {
        var resultDict = new Dictionary<string, string?>(StringComparer.Ordinal);

        try
        {
            string secretPath = SecretManagerPaths.BuildSecretPath(
                    context.ProjectName.ProjectId,
                    context.Config.Region,
                secret.SecretName.SecretId);

            LogSecretLoadStarted(secretPath);

            SecretLoadResult result = await LoadSecretValueAsync(context, secret, secretPath).ConfigureAwait(false);
            string jsonValue = result.Value;

            resultDict.TryAdd(result.Key, jsonValue);

            if (!context.Config.IsRawSecret(secret.SecretName.SecretId))
                TryParseAndFlattenJson(resultDict, result, jsonValue);

            LogSecretLoadCompleted(secretPath);
        }
        catch (RpcException ex)
        {
            LogSecretFailed(ex, secret.SecretName.SecretId, ex.StatusCode);
        }

        return resultDict;
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
        catch (JsonException)
        {
            // If JSON parsing fails, we already have the raw value saved, so just continue
        }
    }

    private async Task<List<Secret>> ListSecretsAsync(ProjectSecretLoadContext context, string parent)
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Critical error during secret loading")]
    private partial void LogLoadFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error loading secrets for {Parent}")]
    private partial void LogProjectLoadFailed(Exception exception, string parent);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Started loading secret {SecretPath}")]
    private partial void LogSecretLoadStarted(string secretPath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Completed loading secret {SecretPath}")]
    private partial void LogSecretLoadCompleted(string secretPath);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to load secret {SecretId}: {StatusCode}")]
    private partial void LogSecretFailed(Exception exception, string secretId, StatusCode statusCode);

    private async Task<SecretLoadResult> LoadSecretValueAsync(
        ProjectSecretLoadContext context,
        Secret secret,
        string secretPath)
    {
        var request = new AccessSecretVersionRequest { Name = secretPath };
        AccessSecretVersionResponse response = await ExecuteWithRetryAsync(
            () => context.Client.AccessSecretVersionAsync(request)).ConfigureAwait(false);

        return new SecretLoadResult(
            secretPath,
            response.Payload.Data.ToStringUtf8(),
            loader.GetKey(secret));
    }

    // Retries transient gRPC failures (ResourceExhausted, Unavailable) with exponential backoff and rethrows the
    // original exception once retries are exhausted or the failure is not transient, so callers log it as before.
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> call)
    {
        Exception? lastException = null;
        Func<CancellationToken, Task<Result<T>>> operation = async _ =>
        {
            try
            {
                return await call().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
                return SecretManagerErrors.FromException(ex);
            }
        };

        Result<T> result = await operation.RetryIfFailed(
            SecretManagerErrors.IsTransient,
            MaxRetryAttempts,
            _retryBaseDelay).ConfigureAwait(false);

        if (result.IsFailure && lastException is not null)
            ExceptionDispatchInfo.Capture(lastException).Throw();

        return result.Value;
    }

    private static void FlattenJson(IDictionary<string, string?> data, JsonElement element, string parentPath)
    {
        JsonValueKind kind = element.ValueKind;

        if (kind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                string newPath = string.IsNullOrEmpty(parentPath)
                    ? property.Name
                    : string.Concat(parentPath, _separator, property.Name);
                FlattenJson(data, property.Value, newPath);
            }
        }
        else if (kind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement item in element.EnumerateArray())
            {
                string newPath = string.Concat(parentPath, _separator, index++);
                FlattenJson(data, item, newPath);
            }
        }
        else
        {
            data[parentPath] = kind == JsonValueKind.Null ? null : element.ToString();
        }
    }
}
