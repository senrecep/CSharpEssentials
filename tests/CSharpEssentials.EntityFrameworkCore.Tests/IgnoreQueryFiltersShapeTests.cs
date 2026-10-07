using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CSharpEssentials.EntityFrameworkCore.Tests;

// The CSE3001 analyzer tests in CSharpEssentials.Tests run on EF Core 9 against a stub of these overloads; this pins the stub to EF Core 10.
public sealed class IgnoreQueryFiltersShapeTests
{
    [Fact]
    public void EntityFrameworkQueryableExtensions_Should_Expose_Parameterless_And_Named_IgnoreQueryFilters_Overloads()
    {
        Type[][] parameterTypes = [.. typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(static method => method.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) && method.IsGenericMethodDefinition)
            .Select(static method => method.GetParameters().Select(static parameter => parameter.ParameterType.IsGenericType
                ? parameter.ParameterType.GetGenericTypeDefinition()
                : parameter.ParameterType).ToArray())];

        parameterTypes.Should().BeEquivalentTo([new[] { typeof(IQueryable<>) }, [typeof(IQueryable<>), typeof(IReadOnlyCollection<>)]]);
    }

    [Fact]
    public void Named_IgnoreQueryFilters_Overload_Should_Take_String_Filter_Keys()
    {
        MethodInfo named = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(static method => method.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) && method.GetParameters().Length == 2);

        named.GetParameters()[1].ParameterType.Should().Be<IReadOnlyCollection<string>>();
    }
}
