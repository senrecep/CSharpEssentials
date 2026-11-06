using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>FluentValidation <c>IsInEnum</c> on the same enum, treated as a plain enum.</summary>
public sealed class FvChannelValidator : AbstractValidator<ChannelRequest>
{
    public FvChannelValidator()
    {
        RuleFor(x => x.Channel).IsInEnum();
    }
}
