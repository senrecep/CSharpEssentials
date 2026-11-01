using FastEndpoints;

using FluentValidation;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.FastEndpointsHost;

// https://fast-endpoints.com/docs/validation
public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}
