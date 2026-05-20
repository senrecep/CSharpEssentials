using System.Text.RegularExpressions;
using CSharpEssentials.Validation.Validators;
using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

// =========================================================================
// CSharpEssentials.Validation Validators
// =========================================================================

// 1. Simple (NotEmpty + EmailAddress + GreaterThan + collection bounds)
public sealed class CseUserValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).NotEmpty().MaxLength(100);
        rules.For(() => model.Email).NotEmpty().EmailAddress();
        rules.For(() => model.Age).GreaterThan(0).LessThan(150);
        rules.For(() => model.Tags).NotEmpty().MinCount(1).MaxCount(10);
        return ValueTask.CompletedTask;
    }
}

// 2. String validators (MinLength / MaxLength / Matches / Must)
public sealed class CseStringValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name)
            .NotEmpty()
            .MinLength(2)
            .MaxLength(100)
            .Matches(@"^[A-Za-z\s]+$");
        rules.For(() => model.Email)
            .NotEmpty()
            .EmailAddress()
            .Must(e => e!.EndsWith(".com", StringComparison.Ordinal), "Email.EndsWith", "Email must end with '.com'.");
        return ValueTask.CompletedTask;
    }
}

// 3. Comparable validators (GreaterThan / GreaterThanOrEqualTo / LessThan / LessThanOrEqualTo / InclusiveBetween)
public sealed class CseComparableValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Age)
            .GreaterThan(0)
            .GreaterThanOrEqualTo(18)
            .LessThan(150)
            .LessThanOrEqualTo(120)
            .InclusiveBetween(18, 65);
        return ValueTask.CompletedTask;
    }
}

// 4. Collection validators (NotEmpty / MinCount / MaxCount / CountBetween)
public sealed class CseCollectionValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Tags).NotEmpty().MinCount(1).MaxCount(10).CountBetween(1, 10);
        return ValueTask.CompletedTask;
    }
}

// 5. Nested validator (SetValidatorAsync — one level)
public sealed class CseAddressValidator : Validator<Address>
{
    protected override ValueTask Configure(Address model, RuleContext<Address> rules, CancellationToken ct = default)
    {
        rules.For(() => model.City).NotEmpty().MaxLength(100);
        rules.For(() => model.ZipCode).NotEmpty().Matches(@"^\d{5}$");
        return ValueTask.CompletedTask;
    }
}

public sealed class CseOrderItemValidator : Validator<OrderItem>
{
    protected override ValueTask Configure(OrderItem model, RuleContext<OrderItem> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Sku).NotEmpty().MaxLength(50);
        rules.For(() => model.Quantity).GreaterThan(0);
        rules.For(() => model.Price).GreaterThan(0);
        return ValueTask.CompletedTask;
    }
}

public sealed class CseNestedValidator : Validator<Order>
{
    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        await rules.For(() => model.ShippingAddress).SetValidatorAsync(new CseAddressValidator(), ct);
    }
}

// 6. Collection item validation (ForEach)
public sealed class CseCollectionItemValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.ForEach(() => model.Items, (item, itemRules) =>
        {
            itemRules.For(() => item.Sku).NotEmpty().MaxLength(50);
            itemRules.For(() => item.Quantity).GreaterThan(0);
            itemRules.For(() => item.Price).GreaterThan(0);
        });
        return ValueTask.CompletedTask;
    }
}

// 7. Conditional validation (if IsBusiness)
public sealed class CseConditionalValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerName).NotEmpty().MaxLength(100);
        rules.For(() => model.CustomerEmail).NotEmpty().EmailAddress();

        if (model.IsBusiness)
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);

        return ValueTask.CompletedTask;
    }
}

// 8. Custom predicate (Must — sync)
public sealed class CseCustomPredicateValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).Must(name => name != "admin", "Name.Reserved", "Name is reserved.");
        return ValueTask.CompletedTask;
    }
}

// 9. Async predicate (MustAsync)
public sealed class CseAsyncPredicateValidator : Validator<User>
{
    protected override async ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        await rules.For(() => model.Name)
            .MustAsync(async (name, c) =>
            {
                await Task.Yield();
                return name != "admin";
            }, "Name.Reserved", "Name is reserved.", ct);
    }
}

// 10. Cascade Continue (accumulate all errors)
public sealed class CseCascadeContinueValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .MinLength(8)
            .Matches(@"[A-Z]")
            .Matches(@"[0-9]");
        return ValueTask.CompletedTask;
    }
}

// 11. Complex model (all rule types combined)
public sealed class CseComplexValidator : Validator<Order>
{
    protected override async ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerName).NotEmpty().MaxLength(100);
        rules.For(() => model.CustomerEmail).NotEmpty().EmailAddress();
        rules.For(() => model.CustomerAge).GreaterThan(0).LessThan(150);
        rules.For(() => model.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .MinLength(8)
            .Matches(@"[A-Z]")
            .Matches(@"[0-9]");
        rules.For(() => model.Tags).NotEmpty().MinCount(1).MaxCount(10);

        if (model.IsBusiness)
            rules.For(() => model.CompanyName).NotEmpty().MaxLength(200);

        await rules.For(() => model.ShippingAddress).SetValidatorAsync(new CseAddressValidator(), ct);

        rules.ForEach(() => model.Items, (item, itemRules) =>
        {
            itemRules.For(() => item.Sku).NotEmpty().MaxLength(50);
            itemRules.For(() => item.Quantity).GreaterThan(0);
            itemRules.For(() => item.Price).GreaterThan(0);
        });
    }
}

// 12. String Length range (Length(min, max))
public sealed class CseStringLengthRangeValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).Length(2, 100);
        rules.For(() => model.Email).Length(5, 254);
        return ValueTask.CompletedTask;
    }
}

// 13. String content (Contains / StartsWith / EndsWith — CSE built-ins vs FV Must lambdas)
public sealed class CseStringContentValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Email)
            .Contains("@")
            .Contains(".")
            .EndsWith(".com");
        return ValueTask.CompletedTask;
    }
}

// 14. ExclusiveBetween
public sealed class CseExclusiveBetweenValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Age).ExclusiveBetween(0, 150);
        return ValueTask.CompletedTask;
    }
}

// 15. Nullable value type validation (int? / decimal?)
public sealed class CseNullableValidator : Validator<Product>
{
    protected override ValueTask Configure(Product model, RuleContext<Product> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Score).NotNull().GreaterThan(0).LessThan(100);
        rules.For(() => model.Price).NotNull().GreaterThan(0m).LessThan(1000m);
        return ValueTask.CompletedTask;
    }
}

// 16. String Equal / NotEqual
public sealed class CseStringEqualityValidator : Validator<Product>
{
    protected override ValueTask Configure(Product model, RuleContext<Product> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Status).NotEqual("banned").Equal("active");
        rules.For(() => model.Code).NotEmpty().NotEqual("INVALID");
        return ValueTask.CompletedTask;
    }
}

// 17. Large collection (ForEach over 50 items)
public sealed class CseLargeCollectionValidator : Validator<Order>
{
    protected override ValueTask Configure(Order model, RuleContext<Order> rules, CancellationToken ct = default)
    {
        rules.ForEach(() => model.Items, (item, itemRules) =>
        {
            itemRules.For(() => item.Sku).NotEmpty().MaxLength(50);
            itemRules.For(() => item.Quantity).GreaterThan(0);
            itemRules.For(() => item.Price).GreaterThan(0);
        });
        return ValueTask.CompletedTask;
    }
}

// 18. Wide model (12 fields)
public sealed class CseWideModelValidator : Validator<WideModel>
{
    protected override ValueTask Configure(WideModel model, RuleContext<WideModel> rules, CancellationToken ct = default)
    {
        rules.For(() => model.F1).NotEmpty().MaxLength(100);
        rules.For(() => model.F2).NotEmpty().MaxLength(100);
        rules.For(() => model.F3).NotEmpty().MaxLength(100);
        rules.For(() => model.F4).NotEmpty().MaxLength(100);
        rules.For(() => model.F5).NotEmpty().MaxLength(100);
        rules.For(() => model.F6).NotEmpty().MaxLength(100);
        rules.For(() => model.F7).NotEmpty().MaxLength(100);
        rules.For(() => model.F8).NotEmpty().MaxLength(100);
        rules.For(() => model.F9).NotEmpty().MaxLength(100);
        rules.For(() => model.F10).NotEmpty().MaxLength(100);
        rules.For(() => model.F11).NotEmpty().MaxLength(100);
        rules.For(() => model.F12).NotEmpty().MaxLength(100);
        return ValueTask.CompletedTask;
    }
}

// 19. Deep nested (3 levels: DeepOrder → DeepAddress → Street)
public sealed class CseStreetValidator : Validator<Street>
{
    protected override ValueTask Configure(Street model, RuleContext<Street> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Line1).NotEmpty().MaxLength(200);
        rules.For(() => model.PostalCode).NotEmpty().Matches(@"^\d{5}$");
        return ValueTask.CompletedTask;
    }
}

public sealed class CseDeepAddressValidator : Validator<DeepAddress>
{
    protected override async ValueTask Configure(DeepAddress model, RuleContext<DeepAddress> rules, CancellationToken ct = default)
    {
        rules.For(() => model.City).NotEmpty().MaxLength(100);
        await rules.For(() => model.Street).SetValidatorAsync(new CseStreetValidator(), ct);
    }
}

public sealed class CseDeepNestedValidator : Validator<DeepOrder>
{
    protected override async ValueTask Configure(DeepOrder model, RuleContext<DeepOrder> rules, CancellationToken ct = default)
    {
        rules.For(() => model.CustomerName).NotEmpty().MaxLength(100);
        await rules.For(() => model.ShippingAddress).SetValidatorAsync(new CseDeepAddressValidator(), ct);
    }
}

// 20. Cascade Stop (default, first failure stops the chain)
public sealed class CseCascadeStopValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Password)
            .NotEmpty()
            .MinLength(8)
            .Matches(@"[A-Z]")
            .Matches(@"[0-9]")
            .Matches(@"[!@#$%]");
        return ValueTask.CompletedTask;
    }
}

// 21. Validator construction cost (ctor overhead)
// CseUserValidator / FvUserValidator are reused from above — benchmarked via new() directly.

// 22. Multiple regex (4 Matches rules, Continue mode)
public sealed class CseMultipleRegexValidator : Validator<User>
{
    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .Matches(@"[A-Z]")
            .Matches(@"[a-z]")
            .Matches(@"[0-9]")
            .Matches(@"[!@#$%^&*]");
        return ValueTask.CompletedTask;
    }
}

// 23. Pre-compiled Regex instance (Matches(Regex) overload)
public sealed class CsePrecompiledRegexValidator : Validator<User>
{
    private static readonly Regex UpperCase = new(@"[A-Z]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex LowerCase = new(@"[a-z]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Digit = new(@"[0-9]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Special = new(@"[!@#$%^&*]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    protected override ValueTask Configure(User model, RuleContext<User> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Password)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .Matches(UpperCase)
            .Matches(LowerCase)
            .Matches(Digit)
            .Matches(Special);
        return ValueTask.CompletedTask;
    }
}

// =========================================================================
// FluentValidation Validators
// =========================================================================

// 1. Simple
public sealed class FvUserValidator : AbstractValidator<User>
{
    public FvUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThan(0).LessThan(150);
        RuleFor(x => x.Tags).NotEmpty().Must(t => t!.Count >= 1).Must(t => t!.Count <= 10);
    }
}

// 2. String validators
public sealed class FvStringValidator : AbstractValidator<User>
{
    public FvStringValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(@"^[A-Za-z\s]+$");
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Must(e => e!.EndsWith(".com", StringComparison.Ordinal));
    }
}

// 3. Comparable validators
public sealed class FvComparableValidator : AbstractValidator<User>
{
    public FvComparableValidator()
    {
        RuleFor(x => x.Age)
            .GreaterThan(0)
            .GreaterThanOrEqualTo(18)
            .LessThan(150)
            .LessThanOrEqualTo(120)
            .InclusiveBetween(18, 65);
    }
}

// 4. Collection validators (Must lambdas — FV has no MinCount/MaxCount built-in)
public sealed class FvCollectionValidator : AbstractValidator<User>
{
    public FvCollectionValidator()
    {
        RuleFor(x => x.Tags)
            .NotEmpty()
            .Must(t => t!.Count >= 1)
            .Must(t => t!.Count <= 10)
            .Must(t => t!.Count >= 1 && t!.Count <= 10);
    }
}

// 5. Nested validator
public sealed class FvAddressValidator : AbstractValidator<Address>
{
    public FvAddressValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ZipCode).NotEmpty().Matches(@"^\d{5}$");
    }
}

public sealed class FvOrderItemValidator : AbstractValidator<OrderItem>
{
    public FvOrderItemValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public sealed class FvNestedValidator : AbstractValidator<Order>
{
    public FvNestedValidator()
    {
        RuleFor(x => x.ShippingAddress!).SetValidator(new FvAddressValidator());
    }
}

// 6. Collection item validation
public sealed class FvCollectionItemValidator : AbstractValidator<Order>
{
    public FvCollectionItemValidator()
    {
        RuleForEach(x => x.Items).SetValidator(new FvOrderItemValidator());
    }
}

// 7. Conditional validation
public sealed class FvConditionalValidator : AbstractValidator<Order>
{
    public FvConditionalValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.IsBusiness);
    }
}

// 8. Custom predicate
public sealed class FvCustomPredicateValidator : AbstractValidator<User>
{
    public FvCustomPredicateValidator()
    {
        RuleFor(x => x.Name)
            .Must(name => name is not "admin")
            .WithMessage("Name is reserved.")
            .WithErrorCode("Name.Reserved");
    }
}

// 9. Async predicate
public sealed class FvAsyncPredicateValidator : AbstractValidator<User>
{
    public FvAsyncPredicateValidator()
    {
        RuleFor(x => x.Name)
            .MustAsync(async (name, c) =>
            {
                await Task.Yield();
                return name != "admin";
            })
            .WithMessage("Name is reserved.")
            .WithErrorCode("Name.Reserved");
    }
}

// 10. Cascade Continue
public sealed class FvCascadeContinueValidator : AbstractValidator<User>
{
    public FvCascadeContinueValidator()
    {
        RuleFor(x => x.Password)
            .Cascade(FluentValidation.CascadeMode.Continue)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]");
    }
}

// 11. Complex model
public sealed class FvComplexValidator : AbstractValidator<Order>
{
    public FvComplexValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.CustomerAge).GreaterThan(0).LessThan(150);
        RuleFor(x => x.Password)
            .Cascade(FluentValidation.CascadeMode.Continue)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]");
        RuleFor(x => x.Tags).NotEmpty().Must(t => t!.Count >= 1).Must(t => t!.Count <= 10);
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.IsBusiness);
        RuleFor(x => x.ShippingAddress!).SetValidator(new FvAddressValidator());
        RuleForEach(x => x.Items).SetValidator(new FvOrderItemValidator());
    }
}

// 12. String length range
public sealed class FvStringLengthRangeValidator : AbstractValidator<User>
{
    public FvStringLengthRangeValidator()
    {
        RuleFor(x => x.Name).Length(2, 100);
        RuleFor(x => x.Email).Length(5, 254);
    }
}

// 13. String content (FV has no Contains/StartsWith/EndsWith — Must lambdas used)
public sealed class FvStringContentValidator : AbstractValidator<User>
{
    public FvStringContentValidator()
    {
        RuleFor(x => x.Email)
            .Must(s => s!.Contains('@'))
            .Must(s => s!.Contains('.'))
            .Must(s => s!.EndsWith(".com", StringComparison.Ordinal));
    }
}

// 14. ExclusiveBetween
public sealed class FvExclusiveBetweenValidator : AbstractValidator<User>
{
    public FvExclusiveBetweenValidator()
    {
        RuleFor(x => x.Age).ExclusiveBetween(0, 150);
    }
}

// 15. Nullable value type validation
public sealed class FvNullableValidator : AbstractValidator<Product>
{
    public FvNullableValidator()
    {
        RuleFor(x => x.Score).NotNull().GreaterThan(0).LessThan(100);
        RuleFor(x => x.Price).NotNull().GreaterThan(0m).LessThan(1000m);
    }
}

// 16. String Equal / NotEqual
public sealed class FvStringEqualityValidator : AbstractValidator<Product>
{
    public FvStringEqualityValidator()
    {
        RuleFor(x => x.Status).NotEqual("banned").Equal("active");
        RuleFor(x => x.Code).NotEmpty().NotEqual("INVALID");
    }
}

// 17. Large collection (50 items)
public sealed class FvLargeCollectionValidator : AbstractValidator<Order>
{
    public FvLargeCollectionValidator()
    {
        RuleForEach(x => x.Items).SetValidator(new FvOrderItemValidator());
    }
}

// 18. Wide model (12 fields)
public sealed class FvWideModelValidator : AbstractValidator<WideModel>
{
    public FvWideModelValidator()
    {
        RuleFor(x => x.F1).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F2).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F3).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F4).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F5).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F6).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F7).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F8).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F9).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F10).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F11).NotEmpty().MaximumLength(100);
        RuleFor(x => x.F12).NotEmpty().MaximumLength(100);
    }
}

// 19. Deep nested (3 levels)
public sealed class FvStreetValidator : AbstractValidator<Street>
{
    public FvStreetValidator()
    {
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PostalCode).NotEmpty().Matches(@"^\d{5}$");
    }
}

public sealed class FvDeepAddressValidator : AbstractValidator<DeepAddress>
{
    public FvDeepAddressValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Street!).SetValidator(new FvStreetValidator());
    }
}

public sealed class FvDeepNestedValidator : AbstractValidator<DeepOrder>
{
    public FvDeepNestedValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingAddress!).SetValidator(new FvDeepAddressValidator());
    }
}

// 20. Cascade Stop (explicit)
public sealed class FvCascadeStopValidator : AbstractValidator<User>
{
    public FvCascadeStopValidator()
    {
        RuleFor(x => x.Password)
            .Cascade(FluentValidation.CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[0-9]")
            .Matches("[!@#$%]");
    }
}

// 22. Multiple regex (4 Matches, Continue mode)
public sealed class FvMultipleRegexValidator : AbstractValidator<User>
{
    public FvMultipleRegexValidator()
    {
        RuleFor(x => x.Password)
            .Cascade(FluentValidation.CascadeMode.Continue)
            .NotEmpty()
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[!@#$%^&*]");
    }
}

// 23. Pre-compiled Regex instance
public sealed class FvPrecompiledRegexValidator : AbstractValidator<User>
{
    private static readonly Regex UpperCase = new(@"[A-Z]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex LowerCase = new(@"[a-z]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Digit = new(@"[0-9]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
    private static readonly Regex Special = new(@"[!@#$%^&*]", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    public FvPrecompiledRegexValidator()
    {
        RuleFor(x => x.Password)
            .Cascade(FluentValidation.CascadeMode.Continue)
            .NotEmpty()
            .Matches(UpperCase)
            .Matches(LowerCase)
            .Matches(Digit)
            .Matches(Special);
    }
}
