using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.GcpSecretManager.Configuration;

/// <summary>
/// Configuration options for Google Cloud Secret Manager integration.
/// </summary>
public sealed record SecretManagerConfigurationOptions
{
    /// <summary>
    /// Gets or sets the path to the Google Cloud credentials JSON file.
    /// </summary>
    public string? CredentialsPath { get; set; }

    /// <summary>
    /// Gets or sets the list of project configurations.
    /// The list is treated as immutable (copy-on-write): <see cref="AddProject"/> replaces it,
    /// so copies made with <c>with</c> never observe each other's changes.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value is <see langword="null"/>.</exception>
    public IReadOnlyList<ProjectSecretConfiguration> Projects
    {
        get;
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    } = [];

    /// <summary>
    /// Gets or sets the logger factory used for diagnostics. Configuration providers are built
    /// before dependency injection exists, so pass a factory explicitly.
    /// When null, nothing is logged.
    /// </summary>
    public ILoggerFactory? LoggerFactory { get; set; }

    /// <summary>
    /// Gets or sets the custom configuration loader.
    /// </summary>
    public ISecretManagerConfigurationLoader? Loader { get; set; }

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
    /// <returns>This options instance for chaining.</returns>
    public SecretManagerConfigurationOptions AddProject(ProjectSecretConfiguration project)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(project);
#else
        if (project is null)
            throw new ArgumentNullException(nameof(project));
#endif
        Projects = [.. Projects, project];
        return this;
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
