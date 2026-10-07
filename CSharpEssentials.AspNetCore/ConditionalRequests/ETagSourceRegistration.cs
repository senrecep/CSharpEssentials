namespace CSharpEssentials.AspNetCore;

/// <summary>
/// One <see cref="ConditionalRequestExtensions.AddETagSource{T, TSource}"/> call: the resource type and a typed call into the
/// source, created where the types are known so no generic type is built at run time.
/// </summary>
internal sealed class ETagSourceRegistration(Type type, Func<IServiceProvider, object, ResourceValidators?> getValidators)
{
    public Type Type { get; } = type;

    public ResourceValidators? GetValidators(IServiceProvider services, object value) => getValidators(services, value);
}
