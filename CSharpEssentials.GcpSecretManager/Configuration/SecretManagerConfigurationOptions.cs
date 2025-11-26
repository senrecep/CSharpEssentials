using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.GcpSecretManager.Configuration;

/// <summary>
/// Configuration options for Google Cloud Secret Manager integration.
/// </summary>
public sealed class SecretManagerConfigurationOptions
{
    /// <summary>
    /// Gets or sets the path to the Google Cloud credentials JSON file.
    /// </summary>
    public string? CredentialsPath { get; init; }

    private readonly List<ProjectSecretConfiguration> _projects = [];

    /// <summary>
    /// Gets the list of project configurations.
    /// </summary>
    public IReadOnlyList<ProjectSecretConfiguration> Projects => _projects;

    /// <summary>
    /// Gets or sets the custom configuration loader.
    /// </summary>
    public ISecretManagerConfigurationLoader? Loader { get; init; }

    /// <summary>
    /// Gets or sets whether to load configuration from appsettings.json.
    /// </summary>
    public bool LoadFromAppSettings { get; set; }

    /// <summary>
    /// Gets or sets the configuration section name in appsettings.json.
    /// </summary>
    public string ConfigurationSectionName { get; set; } = GoogleSecretManagerConfig.SectionName;

    /// <summary>
    /// Gets or sets the number of secrets to load in parallel.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of secrets to retrieve per page.
    /// </summary>
    public int PageSize { get; set; } = 300;

    /// <summary>
    /// Adds a project configuration.
    /// </summary>
    /// <param name="project">The project configuration to add.</param>
    public void AddProject(ProjectSecretConfiguration project)
    {
        ArgumentNullException.ThrowIfNull(project);
        _projects.Add(project);
    }

    internal void LoadFromConfiguration(IConfiguration configuration)
    {
        if (!LoadFromAppSettings)
            return;

        IConfigurationSection section = configuration.GetSection(ConfigurationSectionName);
        if (!section.Exists())
        {
            throw new InvalidOperationException($"Configuration section '{ConfigurationSectionName}' not found in appsettings.json");
        }

        GoogleSecretManagerConfig? config = section.Get<GoogleSecretManagerConfig>();
        if (config?.Projects == null || !config.Projects.Any())
        {
            throw new InvalidOperationException($"No valid project configurations found in '{ConfigurationSectionName}' section");
        }

        foreach (ProjectSecretConfiguration project in config.Projects)
        {
            if (string.IsNullOrEmpty(project.ProjectId))
            {
                throw new InvalidOperationException("ProjectId is required for each project configuration");
            }
            AddProject(project);
        }
    }
}