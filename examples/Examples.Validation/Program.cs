using System.Text.RegularExpressions;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using Microsoft.Extensions.DependencyInjection;

// 1. A validator class
Console.WriteLine("--- 1. Validator<T> ---");
var userValidator = new CreateUserValidator();
Print(await userValidator.ValidateAsync(new CreateUser("ada@example.com", "Ada", 30, Role.Admin, ["dev"])));
Print(await userValidator.ValidateAsync(new CreateUser("not-an-email", "", 7, (Role)42, [])));

// 2. Inline validation without a class
Console.WriteLine("--- 2. Validator.ValidateAsync ---");
Print(await Validator.ValidateAsync(new CreateUser("x@y.z", "A", 20, Role.User, null), (m, rules) =>
{
    rules.For(() => m.Name).NotEmpty().MinLength(2);
    rules.For(() => m.Tags).NotNull();
}));

// 3. Cascade: Stop (default) records one error per chain, Continue records all
Console.WriteLine("--- 3. CascadeMode ---");
Print(await Validator.ValidateAsync(new Password("abc"), (m, rules) =>
    rules.For(() => m.Value)
        .Cascade(CascadeMode.Continue)
        .MinLength(8)
        .Matches("[A-Z]", message: "Must contain an uppercase letter.")
        .Matches(DigitRegex(), message: "Must contain a digit.")));

// 4. Must and MustAsync
Console.WriteLine("--- 4. Must / MustAsync ---");
HashSet<string> takenEmails = ["taken@example.com"];
Print(await Validator.ValidateAsync(new CreateUser("taken@example.com", "admin", 30, Role.User, null), async (m, rules, ct) =>
{
    rules.For(() => m.Name).Must(n => n != "admin", "Name.Reserved", "The name 'admin' is reserved.");
    rules.For(() => m.Age).Must(a => a % 2 == 0);
    await rules.For(() => m.Email).MustAsync(
        async (email, token) => { await Task.Yield(); return !takenEmails.Contains(email!); },
        "Email.Taken", "Email is already registered.", ct);
}));

// 5. Nested objects and collections
Console.WriteLine("--- 5. SetValidatorAsync / ForEach ---");
var order = new Order(
    "C-1",
    new Address("", "34000"),
    BillingAddress: null,
    [new OrderLine("SKU-1", 2), new OrderLine("", 0)],
    Coupon: "  ",
    Kind: OrderKind.Business,
    CompanyName: null);
Print(await new OrderValidator().ValidateAsync(order));

// 6. Composition with Include
Console.WriteLine("--- 6. Include ---");
Print(await new PaidOrderValidator().ValidateAsync(order with { Lines = [new OrderLine("SKU-1", 1)], ShippingAddress = new Address("Istanbul", "34000"), Kind = OrderKind.Personal }));

// 7. Custom rule, manual failure, pre-evaluated value
Console.WriteLine("--- 7. Custom rules ---");
Print(await Validator.ValidateAsync(new CreateUser("a@b.co", "Ada", 31, Role.User, null), (m, rules) =>
{
    rules.For(() => m.Age).Even();
    rules.For(m.Name, "Profile.DisplayName").MaxLength(2);
    rules.AddFailure("User.Policy", "Users cannot be created on Sundays.");
}));

// 8. Railway: validate a Result<T> without unwrapping it
Console.WriteLine("--- 8. ValidateWith / ValidateWithAsync ---");
Result<CreateUser> parsed = new CreateUser("a@b.co", "", 30, Role.User, null);
Print(parsed.ValidateWith((m, rules) => rules.For(() => m.Name).NotEmpty()));
Print(await parsed.ValidateWithAsync(userValidator));
Result<CreateUser> parseFailed = Error.Validation("Json.Invalid", "Body is not valid JSON.");
Print(await parseFailed.ValidateWithAsync(userValidator));

// 9. Error metadata: built-in rules store the property path under "field"
Console.WriteLine("--- 9. field metadata ---");
Result<CreateUser> withField = await userValidator.ValidateAsync(new CreateUser("", "Ada", 30, Role.User, null));
Console.WriteLine($"{withField.FirstError.Code} -> field={withField.FirstError.Metadata?.GetField()}");

// 10. DI registration
Console.WriteLine("--- 10. DI ---");
IServiceCollection services = new ServiceCollection();
services.AddValidator<CreateUser, CreateUserValidator>();
services.AddValidator<CreateUser, CreateUserValidator>(); // no duplicate: TryAddEnumerable
services.AddValidator<Order, OrderValidator>(ServiceLifetime.Singleton);
foreach (ServiceDescriptor d in services)
    Console.WriteLine($"{d.ServiceType.Name.Replace("`1", "")}<{d.ServiceType.GenericTypeArguments[0].Name}> -> {d.ImplementationType?.Name} ({d.Lifetime})");

// 11. The rest of the built-in rules and ForEachAsync
Console.WriteLine("--- 11. More built-in rules ---");
var product = new Product("x-1", "Widget", 0m, 150, null, ["a", "b", "c", "d"], Flags: (Feature)8, Role.User);
Print(await Validator.ValidateAsync(product, async (m, rules, ct) =>
{
    rules.For(() => m.Sku).StartsWith("SKU-").EndsWith("1");
    rules.For(() => m.Name).Contains("Gadget", StringComparison.OrdinalIgnoreCase).NotEqual("Widget").Equal("Widget");
    rules.For(() => m.Price).GreaterThanOrEqualTo(0.01m).ExclusiveBetween(0m, 1000m).NotEqual(0m).Equal(1m);
    rules.For(() => m.Stock).LessThan(100).LessThanOrEqualTo(99);
    rules.For(() => m.DiscontinuedAt).Null().NotNull();
    rules.For(() => m.Tags).MinCount(1).CountBetween(1, 3);
    rules.For(() => m.Flags).HasOnlyDefinedFlags();
    rules.For(() => m.Role).IsOneOf(Role.Admin);
    await rules.ForEachAsync(() => m.Tags, async (tag, tagRules, token) =>
        await tagRules.For(() => tag).MustAsync(async (t, c) => { await Task.Yield(); return t != "d"; }, "Tag.Blocked", "Tag 'd' is blocked.", token), ct);
}, CancellationToken.None));

// 12. Assembly scanning (reflection based; prefer AddValidator in trimmed or AOT apps)
Console.WriteLine("--- 12. AddValidatorsFromAssembly ---");
IServiceCollection scanned = new ServiceCollection().AddValidatorsFromAssemblies([typeof(Program).Assembly]);
Console.WriteLine($"validators found: {scanned.Count}");

static void Print<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        Console.WriteLine("  valid");
        return;
    }
    foreach (Error e in result.Errors)
        Console.WriteLine($"  {e.Code}: {e.Description}");
}

internal partial class Program
{
    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();
}

internal enum Role { User, Admin }
internal enum OrderKind { Personal, Business }
[Flags] internal enum Feature { None = 0, Wifi = 1, Gps = 2 }
internal sealed record Product(string? Sku, string? Name, decimal Price, int Stock, DateTime? DiscontinuedAt, List<string>? Tags, Feature Flags, Role Role);

internal sealed record CreateUser(string? Email, string? Name, int Age, Role Role, List<string>? Tags);
internal sealed record Password(string? Value);
internal sealed record Address(string? City, string? PostalCode);
internal sealed record OrderLine(string? Sku, int Quantity);
internal sealed record Order(string? CustomerId, Address? ShippingAddress, Address? BillingAddress, List<OrderLine>? Lines, string? Coupon, OrderKind Kind, string? CompanyName);

internal sealed class CreateUserValidator : Validator<CreateUser>
{
    protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Age).InclusiveBetween(18, 120);
        rules.For(() => model.Role).IsDefinedEnum();
        rules.For(() => model.Tags).MaxCount(5);
        return ValueTask.CompletedTask;
    }
}

internal sealed class AddressValidator : Validator<Address>
{
    protected override ValueTask Configure(Address model, RuleContext<Address> rules, CancellationToken ct = default)
    {
        rules.For(() => model.City).NotEmpty();
        rules.For(() => model.PostalCode).Length(5, 5);
        return ValueTask.CompletedTask;
    }
}

internal sealed class OrderValidator : Validator<Order>
{
    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerId).NotEmpty();
        rules.For(() => model.Lines).NotEmpty().MaxCount(50);

        await rules.For(() => model.ShippingAddress)
            .Must(a => a is not null, "ShippingAddress.Required", "Shipping address is required.")
            .SetValidatorAsync(new AddressValidator(), ct);
        await rules.For(() => model.BillingAddress).SetValidatorAsync(new AddressValidator(), ct); // null is skipped

        rules.ForEach(() => model.Lines, (line, lineRules) =>
        {
            lineRules.For(() => line.Sku).NotEmpty();
            lineRules.For(() => line.Quantity).GreaterThan(0);
        });

        if (model.Coupon is not null)
            rules.For<string?>(() => model.Coupon).NotEmpty(); // the null check narrows Coupon to string; keep the chain string?

        if (model.Kind == OrderKind.Business)
            rules.For(() => model.CompanyName).NotEmpty();
    }
}

internal sealed class PaidOrderValidator : Validator<Order>
{
    public override int Order => 1;

    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        await Include(new OrderValidator(), model, rules, ct);
        rules.For(() => model.Coupon).Must(c => c is null || c.Trim().Length > 0, "Order.Coupon", "Coupon must not be blank.");
    }
}

internal static class AgeRules
{
    public static RuleChain<T, int> Even<T>(this RuleChain<T, int> chain, string? message = null)
    {
        if (!chain.HasFailed && chain.Value % 2 != 0)
            chain.AddError($"{chain.PropertyName}.Even", message ?? $"'{chain.PropertyName}' must be even.");
        return chain;
    }
}
