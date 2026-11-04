using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The CSE validator with 1 rule(s): Name not empty.</summary>
public sealed class CreateUserValidator1 : Validator<CreateUser>
{
    /// <inheritdoc />
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        return ValueTask.CompletedTask;
    }
}
