using FluentValidation;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The FluentValidation validator with 3 rule(s): Name not empty, Email not empty, Age greater than 0.</summary>
public sealed class CreateUserFluentValidator3 : AbstractValidator<CreateUser>
{
    public CreateUserFluentValidator3()
    {
        RuleFor(static x => x.Name).NotEmpty();
        RuleFor(static x => x.Email).NotEmpty();
        RuleFor(static x => x.Age).GreaterThan(0);
    }
}
