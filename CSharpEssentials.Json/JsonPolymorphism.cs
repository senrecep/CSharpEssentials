using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Json;

/// <summary>
/// The registry of generated polymorphic registrations. Every assembly with <see cref="JsonTypeDiscriminatorAttribute"/> or
/// <see cref="JsonPolymorphicPropertyAttribute"/> registers from a generated module initializer; registrations from several assemblies
/// merge per base type.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class JsonPolymorphism
{
#if NET9_0_OR_GREATER
    private static readonly Lock Gate = new();
#else
    private static readonly object Gate = new();
#endif
    private static readonly Dictionary<Type, List<PolymorphicRegistration>> DerivedTypes = [];
    private static readonly Dictionary<(Type DeclaringType, string MemberName), PolymorphicRegistration> Properties = [];

    /// <summary>Registers the generated entries of one assembly. Registering the same derived type and value again does nothing.</summary>
    /// <param name="registrations">The entries.</param>
    public static void Register(PolymorphicRegistration[] registrations)
    {
        _ = registrations ?? throw new ArgumentNullException(nameof(registrations));

        lock (Gate)
        {
            foreach (PolymorphicRegistration registration in registrations)
            {
                _ = registration ?? throw new ArgumentException("A registration is null.", nameof(registrations));
                if (registration.Derived is null)
                {
                    Properties.TryAdd((registration.Owner, registration.MemberName!), registration);
                    continue;
                }

                if (!DerivedTypes.TryGetValue(registration.Owner, out List<PolymorphicRegistration>? entries))
                {
                    entries = [];
                    DerivedTypes.Add(registration.Owner, entries);
                }

                if (!entries.Exists(entry => entry.Derived == registration.Derived && Equals(entry.TypeDiscriminator, registration.TypeDiscriminator)))
                    entries.Add(registration);
            }
        }
    }

    internal static PolymorphicRegistration[] GetDerivedTypes(Type baseType)
    {
        RunModuleInitializers(baseType);
        lock (Gate)
        {
            return DerivedTypes.TryGetValue(baseType, out List<PolymorphicRegistration>? entries) ? [.. entries] : [];
        }
    }

    internal static PolymorphicRegistration? GetProperty(Type declaringType, string memberName)
    {
        RuntimeHelpers.RunModuleConstructor(declaringType.Module.ModuleHandle);
        lock (Gate)
        {
            return Properties.TryGetValue((declaringType, memberName), out PolymorphicRegistration? entry) ? entry : null;
        }
    }

    /// <summary>
    /// Runs the module initializers of the base type's module and of every loaded assembly marked with
    /// <see cref="JsonPolymorphismModuleAttribute"/>. The runtime runs each one at most once; under native AOT all of them ran at startup.
    /// </summary>
    private static void RunModuleInitializers(Type baseType)
    {
        RuntimeHelpers.RunModuleConstructor(baseType.Module.ModuleHandle);
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDefined(typeof(JsonPolymorphismModuleAttribute), false))
                RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);
        }
    }
}
