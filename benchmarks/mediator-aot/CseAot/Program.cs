using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = [];
services.AddMediator(options =>
{
    options.GenerateTypesAsInternal = true;
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>), typeof(ExceptionHandlingBehavior<,>)];
});
services.AddMediatorValidationOptions();
services.AddValidator<CreateOrder, CreateOrderValidator>();

using ServiceProvider provider = services.BuildServiceProvider();
IMediator mediator = provider.GetRequiredService<IMediator>();
Result<int> valid = await mediator.Send(new CreateOrder("a", 5));
Result<int> invalid = await mediator.Send(new CreateOrder("", 0));
string codes = invalid.IsFailure ? string.Join(",", invalid.Errors.Select(static e => e.Code)) : "none";
bool ok = valid.IsSuccess && valid.Value == 6 && codes == "Name.NotEmpty,Quantity.InclusiveBetween";
Console.WriteLine($"valid {(valid.IsSuccess ? valid.Value.ToString() : "failure")} invalid {codes} {(ok ? "ok" : "MISMATCH")}");
return ok ? 0 : 1;

internal sealed record CreateOrder(string? Name, int Quantity) : ICommand<Result<int>>;

internal sealed class CreateOrderHandler : ICommandHandler<CreateOrder, Result<int>>
{
    public ValueTask<Result<int>> Handle(CreateOrder command, CancellationToken cancellationToken) => new(Result<int>.Success(command.Quantity + 1));
}

internal sealed class CreateOrderValidator : Validator<CreateOrder>
{
    protected override ValueTask Configure(CreateOrder model, RuleContext<CreateOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty();
        rules.For(() => model.Quantity).InclusiveBetween(1, 100);
        return ValueTask.CompletedTask;
    }
}
