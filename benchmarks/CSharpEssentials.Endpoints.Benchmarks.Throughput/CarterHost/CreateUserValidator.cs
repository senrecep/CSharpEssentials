using FluentValidation;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.CarterHost;

// https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}
