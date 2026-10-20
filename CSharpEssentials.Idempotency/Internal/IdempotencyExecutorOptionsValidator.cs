using Microsoft.Extensions.Options;

namespace CSharpEssentials.Idempotency;

/// <summary>Validates <see cref="IdempotencyExecutorOptions"/> every time they are built, including after a reload.</summary>
internal sealed class IdempotencyExecutorOptionsValidator : IValidateOptions<IdempotencyExecutorOptions>
{
    public ValidateOptionsResult Validate(string? name, IdempotencyExecutorOptions options)
    {
        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (InvalidOperationException exception)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }
    }
}
