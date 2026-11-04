using FluentValidation;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>The FluentValidation validator with 1 rule(s): Name not empty.</summary>
public sealed class CreateUserFluentValidator1 : AbstractValidator<CreateUser>
{
    public CreateUserFluentValidator1()
    {
        RuleFor(static x => x.Name).NotEmpty();
    }
}
