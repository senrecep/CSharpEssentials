using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CSharpEssentials.EntityFrameworkCore.DbErrors;

public static class DbErrorTranslationServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="DbErrorTranslation"/> as a singleton. With no other translator registered it uses the
    /// built-in <see cref="SqlStateErrorTranslator"/> only.
    /// </summary>
    public static IServiceCollection AddDbErrorTranslation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<DbErrorTranslation>();
        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="TTranslator"/> as a singleton <see cref="IDbErrorTranslator"/> and adds
    /// <see cref="DbErrorTranslation"/>. Translators run in registration order, before <see cref="SqlStateErrorTranslator"/>.
    /// Registering the same translator type twice has no effect.
    /// </summary>
    public static IServiceCollection AddDbErrorTranslator<TTranslator>(this IServiceCollection services)
        where TTranslator : class, IDbErrorTranslator
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDbErrorTranslator, TTranslator>());
        return services.AddDbErrorTranslation();
    }
}
