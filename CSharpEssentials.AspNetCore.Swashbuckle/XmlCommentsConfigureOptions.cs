using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Includes the XML documentation file of every assembly that declares an API (controller actions and Minimal API
/// handlers), plus the assembly passed to <c>AddSwagger</c>. Files that do not exist are skipped.
/// </summary>
internal sealed class XmlCommentsConfigureOptions(IServiceProvider serviceProvider, Assembly? assembly)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (string path in GetAssemblies().Select(GetXmlPath).Distinct(StringComparer.Ordinal).Where(File.Exists))
            options.IncludeXmlComments(path);
    }

    private IEnumerable<Assembly> GetAssemblies()
    {
        if (assembly is not null)
            yield return assembly;
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is not null)
            yield return entryAssembly;
        IApiDescriptionGroupCollectionProvider? provider = serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>();
        if (provider is null)
            yield break;
        foreach (ApiDescription description in provider.ApiDescriptionGroups.Items.SelectMany(group => group.Items))
        {
            Assembly? declaring = GetDeclaringAssembly(description);
            if (declaring is not null)
                yield return declaring;
        }
    }

    private static Assembly? GetDeclaringAssembly(ApiDescription description)
    {
        MethodInfo? method = description.ActionDescriptor is ControllerActionDescriptor action
            ? action.MethodInfo
            : description.ActionDescriptor.EndpointMetadata.OfType<MethodInfo>().FirstOrDefault();
        return method?.DeclaringType?.Assembly;
    }

    private static string GetXmlPath(Assembly source) =>
        Path.Combine(AppContext.BaseDirectory, $"{source.GetName().Name}.xml");
}
