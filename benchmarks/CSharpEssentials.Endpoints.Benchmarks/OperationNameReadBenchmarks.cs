using BenchmarkDotNet.Attributes;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks;

/// <summary>
/// Reads the endpoint name of all 500 endpoints of an app that is already built and whose names are already resolved, and
/// returns the summed name lengths. <c>CseTypeName</c> reads names made by <c>OperationNaming.TypeName</c>, <c>CseCustom</c>
/// names made by <c>OperationNaming.Custom(type.Name)</c> (plain <c>EndpointNameMetadata</c>), and <c>DirectWithName</c>
/// names set with <c>WithName(...)</c> on hand-written endpoints, a reference row. All three read the same 500 names.
/// The apps are built in setup and disposed in cleanup; nothing else is measured.
/// </summary>
[MemoryDiagnoser]
public class OperationNameReadBenchmarks
{
    private WebApplication _typeNameApp = null!;
    private WebApplication _customApp = null!;
    private WebApplication _directApp = null!;
    private Endpoint[] _typeName = [];
    private Endpoint[] _custom = [];
    private Endpoint[] _direct = [];

    [GlobalSetup]
    public void Setup()
    {
        (_typeNameApp, IReadOnlyList<Endpoint> typeName) = MappingHost.BuildKept(OperationNamingBenchmarks.MapCseTypeName);
        (_customApp, IReadOnlyList<Endpoint> custom) = MappingHost.BuildKept(OperationNamingBenchmarks.MapCseCustom);
        (_directApp, IReadOnlyList<Endpoint> direct) = MappingHost.BuildKept(LoopEndpoints.MapDirectWithName);
        _typeName = [.. typeName];
        _custom = [.. custom];
        _direct = [.. direct];

        int expected = LoopEndpoints.Names.Sum(static name => name.Length);
        int typeNameSum = CseTypeName();
        int customSum = CseCustom();
        int directSum = DirectWithName();
        if (typeNameSum != expected || customSum != expected || directSum != expected)
        {
            throw new InvalidOperationException(
                $"Name lengths differ: expected {expected}, CseTypeName {typeNameSum}, CseCustom {customSum}, DirectWithName {directSum}.");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _typeNameApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _customApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _directApp.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping-options
    [Benchmark(Baseline = true)]
    public int CseTypeName() => SumNames(_typeName);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#mapping-options
    [Benchmark]
    public int CseCustom() => SumNames(_custom);

    // https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.routingendpointconventionbuilderextensions.withname
    [Benchmark]
    public int DirectWithName() => SumNames(_direct);

    private static int SumNames(Endpoint[] endpoints)
    {
        int sum = 0;
        for (int i = 0; i < endpoints.Length; i++)
        {
            sum += endpoints[i].Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName.Length;
        }

        return sum;
    }
}
