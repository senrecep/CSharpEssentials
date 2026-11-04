using CSharpEssentials.AspNetCore.Swagger.Filters;
using System.Collections.Concurrent;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore;

public class SwashbuckleSchemaIdFactoryTests
{
    [Fact]
    public void GetSchemaId_ForSimpleType_ShouldReturnTypeName()
    {
        var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };

        string schemaId = factory.GetSchemaId(typeof(string));

        schemaId.Should().Be("String");
    }

    [Fact]
    public void GetSchemaId_ForGenericType_ShouldReturnFormattedName()
    {
        var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };

        string schemaId = factory.GetSchemaId(typeof(List<string>));

        schemaId.Should().Be("StringList");
    }

    [Fact]
    public void GetSchemaId_ForArrayType_ShouldReturnArraySuffix()
    {
        var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };

        string schemaId = factory.GetSchemaId(typeof(int[]));

        schemaId.Should().Be("Int32Array");
    }

    [Fact]
    public void GetSchemaId_ForSameTypeTwice_ShouldReturnSameId()
    {
        var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };

        string schemaId1 = factory.GetSchemaId(typeof(DateTime));
        string schemaId2 = factory.GetSchemaId(typeof(DateTime));

        schemaId1.Should().Be(schemaId2);
    }

    [Fact]
    public void GetSchemaId_ForNestedGeneric_ShouldReturnFormattedName()
    {
        var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };

        string schemaId = factory.GetSchemaId(typeof(Dictionary<string, int>));

        schemaId.Should().Be("StringInt32Dictionary");
    }

    [Fact]
    public void GetSchemaId_Should_GiveEachTypeOneDistinctId_When_CalledConcurrently()
    {
        Type[] arguments = [typeof(int), typeof(string), typeof(Guid), typeof(DateTime), typeof(decimal), typeof(bool), typeof(long), typeof(byte)];
        Type[] definitions = [typeof(List<>), typeof(HashSet<>), typeof(Queue<>), typeof(Stack<>), typeof(LinkedList<>)];
        Type[] types =
        [
            .. definitions.SelectMany(definition => arguments.Select(argument => definition.MakeGenericType(argument))),
            typeof(FirstScope.Order),
            typeof(SecondScope.Order),
            typeof(FirstScope.Item),
            typeof(SecondScope.Item),
        ];

        for (int round = 0; round < 300; round++)
        {
            var factory = new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false };
            var ids = new ConcurrentDictionary<Type, ConcurrentBag<string>>();

            Parallel.For(0, types.Length * 20, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                Type type = types[i % types.Length];
                ids.GetOrAdd(type, _ => []).Add(factory.GetSchemaId(type));
            });

            ids.Values.Should().AllSatisfy(bag => bag.Distinct().Should().ContainSingle());
            ids.Values.Select(bag => bag.First()).Should().OnlyHaveUniqueItems();
        }
    }

    [Fact]
    public void GetSchemaId_Should_QualifyWithNamespace_ByDefault()
    {
        var factory = new SwashbuckleSchemaIdFactory();

        factory.QualifyWithNamespace.Should().BeTrue();
        factory.GetSchemaId(typeof(string)).Should().Be("System.String");
        factory.GetSchemaId(typeof(List<string>)).Should().Be("System.StringSystem.Collections.Generic.List");
        factory.GetSchemaId(typeof(int[])).Should().Be("System.Int32Array");
    }

    [Fact]
    public void GetSchemaId_Should_NotDependOnFirstSeenOrder_ByDefault()
    {
        var firstSeenFirst = new SwashbuckleSchemaIdFactory();
        var secondSeenFirst = new SwashbuckleSchemaIdFactory();

        firstSeenFirst.GetSchemaId(typeof(FirstScope.Order));
        string afterFirst = firstSeenFirst.GetSchemaId(typeof(SecondScope.Order));
        string beforeFirst = secondSeenFirst.GetSchemaId(typeof(SecondScope.Order));
        secondSeenFirst.GetSchemaId(typeof(FirstScope.Order));

        afterFirst.Should().Be(beforeFirst)
            .And.Be("CSharpEssentials.Tests.AspNetCore.SwashbuckleSchemaIdFactoryTests.SecondScope.Order");
        secondSeenFirst.GetSchemaId(typeof(List<SecondScope.Order[]>))
            .Should().Be("CSharpEssentials.Tests.AspNetCore.SwashbuckleSchemaIdFactoryTests.SecondScope.OrderArraySystem.Collections.Generic.List");
        secondSeenFirst.GetSchemaId(typeof(List<int>[]))
            .Should().Be("System.Int32System.Collections.Generic.ListArray");
    }

    private static class FirstScope
    {
        public sealed class Order;

        public sealed class Item;
    }

    private static class SecondScope
    {
        public sealed class Order;

        public sealed class Item;
    }
}
