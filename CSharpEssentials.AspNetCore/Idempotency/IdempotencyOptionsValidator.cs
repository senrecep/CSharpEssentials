using Microsoft.Extensions.Options;

namespace CSharpEssentials.AspNetCore;

/// <summary>Validates <see cref="IdempotencyOptions"/> every time they are built, including after a reload.</summary>
internal sealed class IdempotencyOptionsValidator : IValidateOptions<IdempotencyOptions>
{
    public ValidateOptionsResult Validate(string? name, IdempotencyOptions options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }
    }
}
