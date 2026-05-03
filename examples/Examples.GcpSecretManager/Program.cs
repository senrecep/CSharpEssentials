using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using Microsoft.Extensions.Configuration;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.GcpSecretManager Example");
Console.WriteLine("========================================\n");

// ============================================================================
// SECRET MANAGER CONFIGURATION OPTIONS
// ============================================================================
Console.WriteLine("--- SecretManagerConfigurationOptions ---");

SecretManagerConfigurationOptions options = new();
options.AddProject(new ProjectSecretConfiguration
{
    ProjectId = "my-gcp-project",
    SecretIds = new[] { "app-secrets", "db-connection" },
    PrefixFilters = new[] { "APP_" }
});

Console.WriteLine($"Projects count: {options.Projects.Count}");
Console.WriteLine($"First ProjectId: {options.Projects[0].ProjectId}");
Console.WriteLine($"First SecretIds: [{string.Join(", ", options.Projects[0].SecretIds)}]");
Console.WriteLine();

// ============================================================================
// CONFIGURATION BUILDER SETUP (DEMONSTRATION)
// ============================================================================
Console.WriteLine("--- Configuration Builder Integration ---");

IConfigurationBuilder builder = new ConfigurationBuilder();

// In a real application, you would call:
// builder.AddGcpSecretManager(gcpOptions =>
// {
//     gcpOptions.CredentialsPath = "/path/to/service-account.json";
//     gcpOptions.AddProject(new ProjectSecretConfiguration
//     {
//         ProjectId = "my-gcp-project",
//         SecretIds = new[] { "app-secrets" }
//     });
// });

Console.WriteLine("Configuration builder prepared for GCP Secret Manager integration.");
Console.WriteLine("In production, secrets would be loaded from Google Cloud Secret Manager.");
Console.WriteLine();

// ============================================================================
// PROJECT SECRET CONFIGURATION
// ============================================================================
Console.WriteLine("--- ProjectSecretConfiguration ---");

ProjectSecretConfiguration project = new()
{
    ProjectId = "production-project",
    PrefixFilters = new[] { "PROD_", "API_" },
    RawSecretPrefixes = new[] { "RAW_" }
};

Console.WriteLine($"ProjectId: {project.ProjectId}");
Console.WriteLine($"PrefixFilters: [{string.Join(", ", project.PrefixFilters)}]");
Console.WriteLine($"IsRawSecret('RAW_KEY'): {project.IsRawSecret("RAW_KEY")}");
Console.WriteLine($"IsRawSecret('PROD_KEY'): {project.IsRawSecret("PROD_KEY")}");
Console.WriteLine();

// ============================================================================
// MULTIPLE PROJECTS
// ============================================================================
Console.WriteLine("--- Multiple Projects ---");

SecretManagerConfigurationOptions multiOptions = new();
multiOptions.AddProject(new ProjectSecretConfiguration
{
    ProjectId = "project-a",
    SecretIds = new[] { "secret-1", "secret-2" },
    PrefixFilters = new[] { "APP_", "DB_" }
});
multiOptions.AddProject(new ProjectSecretConfiguration
{
    ProjectId = "project-b",
    SecretIds = new[] { "api-key" },
    RawSecretPrefixes = new[] { "RAW_" }
});

Console.WriteLine($"Multiple projects: {multiOptions.Projects.Count}");
foreach (var p in multiOptions.Projects)
{
    Console.WriteLine($"  - {p.ProjectId}: Secrets=[{string.Join(", ", p.SecretIds)}]");
}
Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("Note: This demo does not connect to actual GCP.");
Console.WriteLine("========================================");
