using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The rule of <see cref="CustomerAgeValidator"/> with Order 1, so it runs after the Order 0 validators.</summary>
public sealed class CustomerAgeLateValidator : Validator<CreateCustomer>
{
    public override int Order => 1;

    protected override ValueTask Configure(CreateCustomer model, RuleContext<CreateCustomer> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Age).InclusiveBetween(18, 120);
        return ValueTask.CompletedTask;
    }
}
