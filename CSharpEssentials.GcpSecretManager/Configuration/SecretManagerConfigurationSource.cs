using CSharpEssentials.GcpSecretManager.Infrastructure;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.GcpSecretManager.Configuration;

internal sealed class SecretManagerConfigurationSource : IConfigurationSource
{
    private readonly SecretManagerConfigurationOptions _options;
    private readonly IServiceClientHelper _clientHelper;

    internal SecretManagerConfigurationSource(
        SecretManagerConfigurationOptions options,
        IServiceClientHelper? clientHelper = null)
    {
        _options = options;
        _clientHelper = clientHelper ?? new ServiceClientHelper();
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var projectConfigs = _options.Projects
            .Select(CreateProjectContext)
            .ToList();

        ISecretManagerConfigurationLoader loader = _options.Loader ?? new DefaultSecretManagerConfigurationLoader();

        return new SecretManagerConfigurationProvider(projectConfigs, loader, _options);
    }

    private ProjectSecretLoadContext CreateProjectContext(ProjectSecretConfiguration project)
    {
        var projectName = new ProjectName(project.ProjectId);
        SecretManagerServiceClient client = CreateClientForProject(project);
        return new(client, projectName, project);
    }

    private SecretManagerServiceClient CreateClientForProject(ProjectSecretConfiguration project) =>
        (project.Region, _options.CredentialsPath) switch
        {
            (null, null) => _clientHelper.Create(),
            (null, _) => _clientHelper.Create(_options.CredentialsPath),
            (_, null) => _clientHelper.CreateWithRegion(project.Region),
            (_, _) => _clientHelper.CreateWithRegion(_options.CredentialsPath, project.Region)
        };
}
