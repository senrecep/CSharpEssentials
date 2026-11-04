using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The CSE validator with 3 rule(s): Name not empty, Email not empty, Age greater than 0.</summary>
public sealed class CreateUserValidator3 : Validator<CreateUser>
{
    /// <inheritdoc />
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        rules.For(() => model.Email).NotEmpty();
        rules.For(() => model.Age).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}
