using CSharpEssentials.Locking;
using CSharpEssentials.Locking.Testing;

namespace Examples.Mediator.Testing;

public sealed class InProcessResourceLockContractTests
{
    public static TheoryData<string> Scenarios => new(ResourceLockContract.ScenarioNames);

    [Theory]
    [MemberData(nameof(Scenarios))]
    public async Task Scenario(string name)
    {
        Exception? failure = await Record.ExceptionAsync(() => new ResourceLockContract(new InProcessResourceLock()).RunAsync(name));

        Assert.Null(failure);
    }
}
