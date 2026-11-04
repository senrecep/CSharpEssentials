using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Checks <see cref="CreateCustomer.Name"/>, written as the CSharpEssentials.Mediator Readme shows. Order 0.</summary>
public sealed class CustomerNameValidator : Validator<CreateCustomer>
{
    protected override ValueTask Configure(CreateCustomer model, RuleContext<CreateCustomer> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
