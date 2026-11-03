using CSharpEssentials.Enums;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Writes no <c>default</c> for an undefined enum value: Swashbuckle serializes a <c>[DefaultValue]</c> or parameter default
/// with the enum converter before any schema filter runs, and the converter throws <see cref="EnumValueException"/> for
/// values like <c>(OrderStatus)42</c>. Object, array and dictionary contracts of an enum and contracts of other types are returned
/// unchanged.
/// </summary>
internal sealed class EnumDefaultDataContractResolver(ISerializerDataContractResolver inner) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        DataContract contract = inner.GetDataContractForType(type);
        if (!(Nullable.GetUnderlyingType(type) ?? type).IsEnum || contract.DataType is DataType.Object or DataType.Array or DataType.Dictionary)
            return contract;

        Func<object, string> converter = contract.JsonConverter;
        return DataContract.ForPrimitive(contract.UnderlyingType, contract.DataType, contract.DataFormat, value =>
        {
            try
            {
                return converter(value);
            }
            catch (EnumValueException)
            {
                return "null";
            }
        });
    }

    /// <summary>Wraps the registered resolver; a resolver registered by type is left as it is.</summary>
    public static void Decorate(IServiceCollection services)
    {
        int index = services.Count - 1;
        while (index >= 0 && (services[index].ServiceType != typeof(ISerializerDataContractResolver) || services[index].IsKeyedService))
            index--;
        if (index < 0)
            return;

        ServiceDescriptor descriptor = services[index];
        Func<IServiceProvider, ISerializerDataContractResolver>? create = descriptor switch
        {
            { ImplementationInstance: ISerializerDataContractResolver instance } => _ => instance,
            { ImplementationFactory: { } factory } => provider => (ISerializerDataContractResolver)factory(provider),
            _ => null,
        };
        if (create is null)
            return;

        services[index] = ServiceDescriptor.Describe(
            typeof(ISerializerDataContractResolver),
            provider => new EnumDefaultDataContractResolver(create(provider)),
            descriptor.Lifetime);
    }
}
