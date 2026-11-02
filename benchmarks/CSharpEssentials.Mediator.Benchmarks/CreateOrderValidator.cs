using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>The one validator every <see cref="BehaviorBenchmarks"/> variant runs, written as the CSharpEssentials.Mediator Readme shows.</summary>
public sealed class CreateOrderValidator : Validator<CreateOrder>
{
    protected override ValueTask Configure(CreateOrder model, RuleContext<CreateOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        rules.For(() => model.Quantity).InclusiveBetween(1, 100);
        return ValueTask.CompletedTask;
    }
}
