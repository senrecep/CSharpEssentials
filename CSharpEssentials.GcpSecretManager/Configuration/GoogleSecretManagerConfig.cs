namespace CSharpEssentials.GcpSecretManager.Configuration;

internal sealed class GoogleSecretManagerConfig
{
    internal const string SectionName = "GoogleSecretManager";
#if NET8_0_OR_GREATER
    public List<ProjectSecretConfiguration> Projects { get; set; } = [];
#else
    public List<ProjectSecretConfiguration> Projects { get; set; } = new List<ProjectSecretConfiguration>();
#endif
}