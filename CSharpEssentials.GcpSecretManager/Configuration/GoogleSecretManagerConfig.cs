using CSharpEssentials.GcpSecretManager.Models;

namespace CSharpEssentials.GcpSecretManager.Configuration.Internal;

internal sealed class GoogleSecretManagerConfig
{
    internal const string SectionName = "GoogleSecretManager";
    public List<ProjectSecretConfiguration> Projects { get; set; } = [];
}