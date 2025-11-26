namespace CSharpEssentials.GcpSecretManager.Infrastructure;

internal static class SecretManagerPaths
{
    internal static string BuildParentPath(string projectId, string? region)
        => string.IsNullOrEmpty(region)
            ? $"projects/{projectId}"
            : $"projects/{projectId}/locations/{region}";

    internal static string BuildSecretPath(string projectId, string? region, string secretId)
        => string.IsNullOrEmpty(region)
            ? $"projects/{projectId}/secrets/{secretId}/versions/latest"
            : $"projects/{projectId}/locations/{region}/secrets/{secretId}/versions/latest";
}