#if NET10_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Resolves Microsoft.Extensions.Validation's generated type info once per benchmark setup:
/// <c>AddValidation()</c>, then <c>ValidationOptions.TryGetValidatableTypeInfo</c>
/// (https://github.com/dotnet/aspnetcore/blob/v10.0.14/src/Validation/src/ValidationOptions.cs).
/// </summary>
public static class MeValidation
{
    public static (ValidationOptions Options, IValidatableInfo Info) Resolve(Type type)
    {
        using ServiceProvider provider = new ServiceCollection().AddValidation().BuildServiceProvider();
        ValidationOptions options = provider.GetRequiredService<IOptions<ValidationOptions>>().Value;
        if (!options.TryGetValidatableTypeInfo(type, out IValidatableInfo? info))
            throw new InvalidOperationException($"Microsoft.Extensions.Validation has no generated type info for {type.Name}.");
        return (options, info);
    }
}
#endif
