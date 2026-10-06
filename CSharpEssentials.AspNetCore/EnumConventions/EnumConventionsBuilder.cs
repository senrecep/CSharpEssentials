using CSharpEssentials.Enums;
using CSharpEssentials.Errors;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Returned by <see cref="EnumConventionsExtensions.AddEnumConventions"/> to configure the enum conventions of the host further.
/// </summary>
public sealed class EnumConventionsBuilder
{
    private readonly EnumConventionsRegistration _registration;

    internal EnumConventionsBuilder(IServiceCollection services, EnumConventionsRegistration registration)
    {
        Services = services;
        _registration = registration;
    }

    /// <summary>The service collection.</summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Sets how a rejected enum value becomes an <see cref="Error"/>, for route, query, header and form values and for JSON
    /// request bodies alike. The key is the parameter name (binding) or the JSON path without <c>$.</c> (body).
    /// The default is <c>Error.Validation(code: key, description: error.Message)</c>.
    /// </summary>
    /// <param name="errorFactory">Creates the error from the rejected value and its key.</param>
    /// <returns>This builder.</returns>
    public EnumConventionsBuilder ConfigureErrors(Func<EnumValueError, string, Error> errorFactory)
    {
        _registration.ErrorFactory = errorFactory ?? throw new ArgumentNullException(nameof(errorFactory));
        return this;
    }
}
