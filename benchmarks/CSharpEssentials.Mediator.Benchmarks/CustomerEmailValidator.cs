using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>Checks <see cref="CreateCustomer.Email"/>, written as the CSharpEssentials.Mediator Readme shows. Order 0.</summary>
public sealed class CustomerEmailValidator : Validator<CreateCustomer>
{
    protected override ValueTask Configure(CreateCustomer model, RuleContext<CreateCustomer> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
