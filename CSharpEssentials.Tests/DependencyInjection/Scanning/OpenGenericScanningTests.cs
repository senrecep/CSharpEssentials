using System.Reflection;
using System.Reflection.Emit;
using CSharpEssentials.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.DependencyInjection.Scanning;

public class OpenGenericScanningTests
{
    public interface IScanMarker;

    public class ScanBase<T>
    {
        public T? Value { get; set; }
    }

    private static AssemblyBuilder CreateGenericAssembly(Type? openBase, Type serviceType)
    {
        string name = "GenericRegistrations" + Guid.NewGuid().ToString("N");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.RunAndCollect);
        TypeBuilder type = assembly.DefineDynamicModule(name).DefineType("Generic`1", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);
        GenericTypeParameterBuilder parameter = type.DefineGenericParameters("T")[0];
        type.AddInterfaceImplementation(typeof(IScanMarker));
        if (openBase is null)
        {
            type.DefineDefaultConstructor(MethodAttributes.Public);
        }
        else
        {
            Type parent = openBase.MakeGenericType(parameter);
            type.SetParent(parent);
            ConstructorBuilder constructor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            ILGenerator il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, TypeBuilder.GetConstructor(parent, openBase.GetConstructor(Type.EmptyTypes)!));
            il.Emit(OpCodes.Ret);
        }

        type.SetCustomAttribute(new CustomAttributeBuilder(typeof(RegisterScopedAttribute).GetConstructor([typeof(Type)])!, [serviceType]));
        type.CreateType();
        return assembly;
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_RegisterOpenGeneric_When_ServiceTypeIsOpenGenericBaseClass()
    {
        ServiceCollection services = new();
        Assembly assembly = CreateGenericAssembly(typeof(ScanBase<>), typeof(ScanBase<>));

        services.AddServicesFromAssemblies(assembly);

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<ScanBase<int>>().GetType().Should().Match<Type>(type =>
            type.Name == "Generic`1" && type.GenericTypeArguments.Single() == typeof(int));
    }

    [Fact]
    public void AddServicesFromAssemblies_Should_ThrowWithoutMutation_When_OpenGenericDoesNotMapServiceType()
    {
        ServiceCollection services = new();
        Assembly assembly = CreateGenericAssembly(openBase: null, typeof(ScanBase<>));

        Action act = () => services.AddServicesFromAssemblies(assembly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Generic`1*cannot be registered as*ScanBase`1*with its type parameters mapped one-to-one in order.");
        services.Should().BeEmpty();
    }
}
