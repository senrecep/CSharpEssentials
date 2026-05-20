using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Validators;

Console.WriteLine("========================================");
Console.WriteLine("CSharpEssentials.Validation Example");
Console.WriteLine("========================================\n");

// ============================================================================
// 1. STRING VALIDATORS
// ============================================================================
Console.WriteLine("--- 1. String Validators ---");

Result<CreateUserCommand> stringResult = await Validator.ValidateAsync(
    new CreateUserCommand("not-an-email", "", 25, "secret"),
    (m, rules) =>
    {
        rules.For(() => m.Email).NotEmpty().EmailAddress();
        rules.For(() => m.Name).NotEmpty().MinLength(2).MaxLength(100);
    });

stringResult.Switch(
    onSuccess: v => Console.WriteLine($"Valid: {v.Email}"),
    onError: errors =>
    {
        foreach (var e in errors)
            Console.WriteLine($"  [{e.Code}] {e.Description}");
    });

Console.WriteLine();

// ============================================================================
// 2. COMPARABLE VALIDATORS (int, decimal, DateTime)
// ============================================================================
Console.WriteLine("--- 2. Comparable Validators ---");

Result<ProductModel> comparableResult = await Validator.ValidateAsync(
    new ProductModel("SKU-001", -5.00m, 0, null),
    (m, rules) =>
    {
        rules.For(() => m.Price).GreaterThan(0m);
        rules.For(() => m.Stock).GreaterThanOrEqualTo(0);
    });

comparableResult.Switch(
    onSuccess: v => Console.WriteLine($"Valid product: {v.Sku}"),
    onError: errors =>
    {
        foreach (var e in errors)
            Console.WriteLine($"  [{e.Code}] {e.Description}");
    });

Console.WriteLine();

// ============================================================================
// 3. NULLABLE STRUCT VALIDATORS (int?, DateTime?)
// ============================================================================
Console.WriteLine("--- 3. Nullable Struct Validators ---");

Result<OrderModel> nullableResult = await Validator.ValidateAsync(
    new OrderModel("CUST-1", null, null, -1),
    (m, rules) =>
    {
        // null is silently skipped — no error for missing Priority
        // only fires when Priority has a value AND fails the rule
        rules.For(() => m.Priority).GreaterThan(0);
    });

Console.WriteLine($"Priority=-1 result: IsFailure={nullableResult.IsFailure}");
// Priority=-1 → fails GreaterThan(0)

Result<OrderModel> nullableSkipResult = await Validator.ValidateAsync(
    new OrderModel("CUST-1", null, null, null),
    (m, rules) => rules.For(() => m.Priority).GreaterThan(0));

Console.WriteLine($"Priority=null result: IsFailure={nullableSkipResult.IsFailure}");
// Priority=null → silently skipped → success

Console.WriteLine();

// ============================================================================
// 4. COLLECTION VALIDATORS (List<T>?, T[]?, IEnumerable<T>?)
// ============================================================================
Console.WriteLine("--- 4. Collection Validators ---");

Result<OrderModel> collectionResult = await Validator.ValidateAsync(
    new OrderModel("CUST-1", [], null, null),
    (m, rules) =>
    {
        rules.For(() => m.Tags).NotEmpty();
    });

Console.WriteLine($"Empty tags NotEmpty: IsFailure={collectionResult.IsFailure}");

Result<ProductModel> arrayResult = await Validator.ValidateAsync(
    new ProductModel("SKU-1", 10m, 5, ["img1.jpg", "img2.jpg", "img3.jpg", "img4.jpg", "img5.jpg", "img6.jpg"]),
    (m, rules) =>
    {
        rules.For(() => m.Images).MinCount(1).MaxCount(5);
    });

Console.WriteLine($"6 images MaxCount(5): IsFailure={arrayResult.IsFailure}");

Result<OrderModel> countBetweenResult = await Validator.ValidateAsync(
    new OrderModel("CUST-1", ["a", "b", "c", "d", "e", "f"], null, null),
    (m, rules) =>
    {
        rules.For(() => m.Tags).CountBetween(1, 5);
    });

Console.WriteLine($"6 tags CountBetween(1,5): Code={countBetweenResult.FirstError.Code}");

Console.WriteLine();

// ============================================================================
// 5. CASCADE MODE — collect ALL errors for a field
// ============================================================================
Console.WriteLine("--- 5. CascadeMode.Continue ---");

Result<CreateUserCommand> cascadeResult = await Validator.ValidateAsync(
    new CreateUserCommand("u@ok.com", "Alice", 30, "weak"),
    (m, rules) =>
    {
        rules.For(() => m.Password)
            .Cascade(CascadeMode.Continue)
            .MinLength(8)
            .Matches(@"[A-Z]", message: "Must contain an uppercase letter.")
            .Matches(@"[0-9]", message: "Must contain a digit.");
    });

cascadeResult.Switch(
    onSuccess: _ => Console.WriteLine("Password valid"),
    onError: errors =>
    {
        Console.WriteLine($"Password has {errors.Length} issue(s):");
        foreach (var e in errors)
            Console.WriteLine($"  {e.Description}");
    });

Console.WriteLine();

// ============================================================================
// 6. MUST — custom predicate
// ============================================================================
Console.WriteLine("--- 6. Must / MustAsync ---");

Result<CreateUserCommand> mustResult = await Validator.ValidateAsync(
    new CreateUserCommand("admin@example.com", "admin", 30, "P@ssw0rd"),
    (m, rules) =>
    {
        rules.For(() => m.Name)
            .NotEmpty()
            .Must(name => name != "admin", "Name.Reserved", "The name 'admin' is reserved.");
    });

mustResult.Switch(
    onSuccess: _ => Console.WriteLine("Name ok"),
    onError: errors => Console.WriteLine($"  [{errors[0].Code}] {errors[0].Description}"));

Console.WriteLine();

// ============================================================================
// 7. NESTED OBJECT VALIDATION — SetValidatorAsync (no ! required)
// ============================================================================
Console.WriteLine("--- 7. Nested Object Validation ---");

AddressValidator addressValidator = new();

// Non-null address with invalid city
Result<OrderModel> nestedFail = await Validator.ValidateAsync(
    new OrderModel("CUST-1", null, new AddressModel("", "34000"), null),
    async (m, rules, ct) =>
        await rules.For(() => m.ShippingAddress).SetValidatorAsync(addressValidator, ct));

Console.WriteLine($"Empty city → Code: {nestedFail.FirstError.Code}");

// Null address — silently skipped, no error
Result<OrderModel> nestedSkip = await Validator.ValidateAsync(
    new OrderModel("CUST-1", null, null, null),
    async (m, rules, ct) =>
        await rules.For(() => m.ShippingAddress).SetValidatorAsync(addressValidator, ct));

Console.WriteLine($"Null ShippingAddress → IsSuccess: {nestedSkip.IsSuccess}");

Console.WriteLine();

// ============================================================================
// 8. FOREACH — validate collection items
// ============================================================================
Console.WriteLine("--- 8. ForEach / ForEachAsync ---");

Result<OrderModel> forEachResult = await Validator.ValidateAsync(
    new OrderModel("CUST-1", ["valid", "", "also-valid", ""], null, null),
    (m, rules) =>
        rules.ForEach(() => m.Tags, (tag, tagRules) =>
            tagRules.For(() => tag).NotEmpty()));

forEachResult.Switch(
    onSuccess: _ => Console.WriteLine("All tags valid"),
    onError: errors =>
    {
        Console.WriteLine($"{errors.Length} tag error(s):");
        foreach (var e in errors)
            Console.WriteLine($"  [{e.Code}]");
    });

Console.WriteLine();

// ============================================================================
// 9. NATIVE CONDITIONAL RULES — plain C# if/switch
// ============================================================================
Console.WriteLine("--- 9. Conditional Rules ---");

OrderValidator urgentValidator = new();

Result<OrderModel> urgentResult = await urgentValidator.ValidateAsync(
    new OrderModel(null, null, null, 1));

Console.WriteLine($"Urgent order, missing CustomerId → Code: {urgentResult.FirstError.Code}");

Result<OrderModel> normalResult = await urgentValidator.ValidateAsync(
    new OrderModel(null, null, null, 5));

Console.WriteLine($"Normal order, missing CustomerId → Code: {normalResult.FirstError.Code}");

Console.WriteLine();

// ============================================================================
// 10. INCLUDE — validator composition
// ============================================================================
Console.WriteLine("--- 10. Include (Composition) ---");

FullOrderValidator fullValidator = new();

Result<OrderModel> composedResult = await fullValidator.ValidateAsync(
    new OrderModel(null, [], null, null));

composedResult.Switch(
    onSuccess: _ => Console.WriteLine("Order valid"),
    onError: errors =>
    {
        Console.WriteLine($"{errors.Length} error(s) from composed validator:");
        foreach (var e in errors)
            Console.WriteLine($"  [{e.Code}]");
    });

Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("Demo complete.");
Console.WriteLine("========================================");

// ============================================================================
// MODELS AND VALIDATOR CLASSES
// ============================================================================

record AddressModel(string? City, string? ZipCode);
record CreateUserCommand(string? Email, string? Name, int Age, string? Password);
record OrderModel(string? CustomerId, List<string?>? Tags, AddressModel? ShippingAddress, int? Priority);
record ProductModel(string? Sku, decimal Price, int? Stock, string[]? Images);

sealed class AddressValidator : Validator<AddressModel>
{
    protected override ValueTask Configure(AddressModel model, RuleContext<AddressModel> rules, CancellationToken ct = default)
    {
        rules.For(() => model.City).NotEmpty();
        rules.For(() => model.ZipCode).NotEmpty().MinLength(3).MaxLength(10);
        return ValueTask.CompletedTask;
    }
}

sealed class OrderValidator : Validator<OrderModel>
{
    protected override ValueTask Configure(OrderModel model, RuleContext<OrderModel> rules, CancellationToken ct = default)
    {
        // Always required
        rules.For(() => model.CustomerId).NotEmpty();

        // Conditional — Priority=1 means urgent, require ShippingAddress
        if (model.Priority == 1)
            rules.For(() => model.ShippingAddress).Must(a => a is not null, "ShippingAddress.Required", "Urgent orders require a shipping address.");

        return ValueTask.CompletedTask;
    }
}

sealed class TagsValidator : Validator<OrderModel>
{
    protected override ValueTask Configure(OrderModel model, RuleContext<OrderModel> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Tags).MaxCount(10);
        return ValueTask.CompletedTask;
    }
}

sealed class FullOrderValidator : Validator<OrderModel>
{
    protected override async ValueTask Configure(OrderModel model, RuleContext<OrderModel> rules, CancellationToken ct = default)
    {
        await Include(new OrderValidator(), model, rules, ct);
        await Include(new TagsValidator(), model, rules, ct);
    }
}
