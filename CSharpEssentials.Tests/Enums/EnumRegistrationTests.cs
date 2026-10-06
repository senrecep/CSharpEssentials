using System.Runtime.Loader;
using CSharpEssentials.Enums;
using CSharpEssentials.Tests.Fixtures.EnumsContracts;
using FluentAssertions;

namespace CSharpEssentials.Tests.Enums;

/// <summary>
/// The fixture assembly holds only enum types and is referenced here by type only, so its module initializer has not run when a
/// lookup by <see cref="Type"/> reaches it.
/// </summary>
public class EnumRegistrationTests
{
    [Fact]
    public void TryGet_Should_Find_Metadata_Of_A_Type_Only_Assembly()
    {
        bool found = EnumMetadata.TryGet(typeof(ContractStatus), out IEnumInfo? info);

        found.Should().BeTrue();
        info!.EnumType.Should().Be<ContractStatus>();
    }

    [Fact]
    public void IsRegistered_Should_Be_True_For_A_Type_Only_Assembly()
    {
        bool registered = EnumMetadata.IsRegistered(typeof(ContractStatus));

        registered.Should().BeTrue();
    }

    [Fact]
    public void Get_Should_Return_Metadata_Of_A_Type_Only_Assembly()
    {
        EnumInfo<ContractStatus> info = EnumMetadata.Get<ContractStatus>();

        info.Fallback!.Value.Should().Be(ContractStatus.Unknown);
    }

    [Fact]
    public void Wire_Names_Should_Use_The_Naming_Of_The_Declaring_Project()
    {
        bool found = EnumMetadata.TryGet(typeof(ContractStatus), out IEnumInfo? info);

        found.Should().BeTrue();
        info!.WireNames.Should().Equal("pending-approval", "http-shipped", "unknown");
    }

    [Fact]
    public void EnumValueFormatter_Should_Format_A_Value_Of_A_Type_Only_Assembly()
    {
        bool formatted = EnumValueFormatter.TryFormat(ContractStatus.HTTPShipped, EnumConventions.Default, out string? text);

        formatted.Should().BeTrue();
        text.Should().Be("http-shipped");
    }

    [Fact]
    public void Parallel_First_Lookups_Should_All_Find_Metadata()
    {
        const int threads = 16;
        string path = typeof(ContractStatus).Assembly.Location;

        for (int round = 0; round < 20; round++)
        {
            // A fresh load context gives a module whose initializer has not run yet.
            Type enumType = new AssemblyLoadContext("enum-registration-race-" + round)
                .LoadFromAssemblyPath(path)
                .GetType(typeof(ContractStatus).FullName!, throwOnError: true)!;
            using Barrier barrier = new(threads);
            Task<bool>[] lookups = new Task<bool>[threads];
            for (int i = 0; i < threads; i++)
            {
                lookups[i] = Task.Factory.StartNew(
                    () =>
                    {
                        barrier.SignalAndWait();
                        return EnumMetadata.TryGet(enumType, out IEnumInfo? info) && info.EnumType == enumType;
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }

            Task.WaitAll(lookups);
            lookups.Select(static lookup => lookup.Result).Should().AllBeEquivalentTo(true);
        }
    }
}
