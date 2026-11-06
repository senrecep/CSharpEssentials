using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotentAttributeTests
{
    [Fact]
    public async Task Mode_Argument_Should_Turn_Idempotency_Off_For_The_Action()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/mvc/overrides/mode-off?outcome=201");
        IdempotencyResponse second = await host.PostAsync("/mvc/overrides/mode-off?outcome=201");

        first.StatusCode.Should().Be(201);
        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }
}
