using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Extensions;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

Section("1. Options and projects");
{
    var options = new SecretManagerConfigurationOptions { BatchSize = 5, PageSize = 100 }
        .AddProject(new ProjectSecretConfiguration { ProjectId = "app-prod", PrefixFilters = ["MyApp__"], SecretIds = ["stripe-api-key"] })
        .AddProject(new ProjectSecretConfiguration { ProjectId = "shared-eu", Region = "europe-west1", RawSecretPrefixes = ["cert-"] });
    foreach (ProjectSecretConfiguration project in options.Projects)
        Console.WriteLine($"{project.ProjectId}: region={project.Region ?? "(global)"}, prefixes=[{string.Join(", ", project.PrefixFilters)}], ids=[{string.Join(", ", project.SecretIds)}]");
    options.CredentialsPath = null;          // null: Application Default Credentials
    options.LoadFromAppSettings = true;      // also read projects from the section below
    options.ConfigurationSectionName = "Secrets";
    options.Loader = new DashLoader();       // replaces the __ mapping and the filters
    Console.WriteLine($"BatchSize={options.BatchSize}, PageSize={options.PageSize}, section={options.ConfigurationSectionName}, appsettings={options.LoadFromAppSettings}, loader={options.Loader.GetType().Name}");
}

Section("2. Raw secrets are not flattened as JSON");
{
    var project = new ProjectSecretConfiguration { ProjectId = "app-prod", RawSecretIds = ["service-account"], RawSecretPrefixes = ["cert-"] };
    foreach (string id in new[] { "service-account", "cert-api", "MyApp__Db" })
        Console.WriteLine($"IsRawSecret({id}) = {project.IsRawSecret(id)}");
}

Section("3. Validation happens before Google Cloud is contacted");
{
    Try("no callback, no section", () => new ConfigurationManager().AddGcpSecretManager());
    Try("callback without a project", () => new ConfigurationManager().AddGcpSecretManager(o => o.BatchSize = 5));
    Try("BatchSize = 0", () => new SecretManagerConfigurationOptions().BatchSize = 0);
    Try("PageSize = -1", () => new SecretManagerConfigurationOptions().PageSize = -1);

    var configuration = new ConfigurationManager();
    configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["GoogleSecretManager:Projects:0:PrefixFilters:0"] = "MyApp__" });
    Try("section without ProjectId", () => configuration.AddGcpSecretManager());
}

Section("4. Custom ISecretManagerConfigurationLoader");
{
    var project = new ProjectSecretConfiguration { ProjectId = "app-prod", PrefixFilters = ["myapp--"] };
    var loader = new DashLoader();
    foreach (string id in new[] { "myapp--db--password", "myapp--smtp--host", "billing--key" })
    {
        var secret = new Secret { SecretName = new SecretName(project.ProjectId, id) };
        Console.WriteLine($"{id,-20} load={loader.ShouldLoadSecret(secret, project),-5} key={loader.GetKey(secret)}");
    }
}

Section("5. Loading real secrets (opt-in)");
{
    // Set CSE_GCP_PROJECT to a project you can read, with Application Default Credentials configured.
    string? projectId = Environment.GetEnvironmentVariable("CSE_GCP_PROJECT");
    if (string.IsNullOrEmpty(projectId))
    {
        Console.WriteLine("Skipped: set CSE_GCP_PROJECT to load secrets from Google Cloud.");
    }
    else
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole().SetMinimumLevel(LogLevel.Information));
        var configuration = new ConfigurationManager();
        configuration.AddGcpSecretManager(options =>
        {
            options.LoggerFactory = loggerFactory;
            options.CredentialsPath = Environment.GetEnvironmentVariable("CSE_GCP_CREDENTIALS"); // optional key file
            options.AddProject(new ProjectSecretConfiguration
            {
                ProjectId = projectId,
                PrefixFilters = [Environment.GetEnvironmentVariable("CSE_GCP_PREFIX") ?? ""],
            });
        });
        foreach (KeyValuePair<string, string?> pair in configuration.AsEnumerable().OrderBy(p => p.Key))
            Console.WriteLine($"{pair.Key} = {(pair.Value is null ? "(section)" : new string('*', Math.Min(pair.Value.Length, 8)))}");
    }
}

Console.WriteLine();
Console.WriteLine("Done.");

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"--- {title} ---");
}

static void Try(string name, Action action)
{
    try
    {
        action();
        Console.WriteLine($"{name}: no exception");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{name}: {ex.GetType().Name}");
    }
}

public sealed class DashLoader : ISecretManagerConfigurationLoader
{
    public string GetKey(Secret secret) => GetKey(secret.SecretName.SecretId);

    public string GetKey(string keyId) => keyId.Replace("--", ":");

    public bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration project)
    {
        string id = secret.SecretName.SecretId;
        bool unfiltered = project.PrefixFilters.Count == 0 && project.SecretIds.Count == 0;
        return unfiltered || project.SecretIds.Contains(id) || project.PrefixFilters.Any(p => id.StartsWith(p, StringComparison.Ordinal));
    }
}
