using System.Text.Json;
using CSharpEssentials.EntityFrameworkCore.Outbox;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Outbox;

public sealed class OutboxRegistryOptionsTests
{
    [Fact]
    public void Add_Should_LeaveCallerOptionsEditable_When_OptionsArePassed()
    {
        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        new OutboxEventTypeRegistry().Add<LegacyEvent>("legacy", options);

        options.IsReadOnly.Should().BeFalse();
    }
}
