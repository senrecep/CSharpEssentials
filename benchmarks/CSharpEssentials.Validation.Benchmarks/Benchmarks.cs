using BenchmarkDotNet.Attributes;
using FluentValidation;

namespace CSharpEssentials.Validation.Benchmarks;

// =========================================================================
// Shared test data
// =========================================================================

public static class TestData
{
    public static User ValidUser => new()
    {
        Name = "John Doe",
        Email = "john.doe@example.com",
        Age = 30,
        Tags = ["customer", "premium"]
    };

    public static User InvalidUser => new()
    {
        Name = "",
        Email = "not-an-email",
        Age = -1,
        Tags = []
    };

    public static User AdminUser => new()
    {
        Name = "admin",
        Email = "admin@example.com",
        Age = 30,
        Tags = ["admin"]
    };

    public static User WeakPasswordUser => new()
    {
        Name = "Jane",
        Email = "jane@example.com",
        Age = 25,
        Tags = ["user"],
        Password = "weak"
    };

    public static User ValidUserWithPassword => new()
    {
        Name = "John Doe",
        Email = "john.doe@example.com",
        Age = 30,
        Tags = ["customer", "premium"],
        Password = "StrongP@ss1!"
    };

    public static Order ValidOrder => new()
    {
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        CustomerAge = 30,
        IsBusiness = false,
        Password = "StrongP@ss1",
        Tags = ["customer", "premium"],
        ShippingAddress = new Address { City = "New York", ZipCode = "10001" },
        Items =
        [
            new OrderItem { Sku = "SKU001", Quantity = 2, Price = 10.0m },
            new OrderItem { Sku = "SKU002", Quantity = 1, Price = 25.0m }
        ]
    };

    public static Order InvalidOrder => new()
    {
        CustomerName = "",
        CustomerEmail = "bad-email",
        CustomerAge = -1,
        IsBusiness = true,
        CompanyName = null,
        Password = "weak",
        Tags = [],
        ShippingAddress = new Address { City = "", ZipCode = "bad" },
        Items =
        [
            new OrderItem { Sku = "", Quantity = -1, Price = -5.0m }
        ]
    };

    public static Order BusinessOrder => new()
    {
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        CustomerAge = 30,
        IsBusiness = true,
        CompanyName = "Acme Inc.",
        Password = "StrongP@ss1",
        Tags = ["customer", "premium"],
        ShippingAddress = new Address { City = "New York", ZipCode = "10001" },
        Items =
        [
            new OrderItem { Sku = "SKU001", Quantity = 2, Price = 10.0m },
            new OrderItem { Sku = "SKU002", Quantity = 1, Price = 25.0m }
        ]
    };

    public static Order PersonalOrder => new()
    {
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        CustomerAge = 30,
        IsBusiness = false,
        Password = "StrongP@ss1",
        Tags = ["customer", "premium"],
        ShippingAddress = new Address { City = "New York", ZipCode = "10001" },
        Items =
        [
            new OrderItem { Sku = "SKU001", Quantity = 2, Price = 10.0m },
            new OrderItem { Sku = "SKU002", Quantity = 1, Price = 25.0m }
        ]
    };

    public static Order LargeOrder => new()
    {
        CustomerName = "John Doe",
        CustomerEmail = "john.doe@example.com",
        CustomerAge = 30,
        IsBusiness = false,
        Password = "StrongP@ss1",
        Tags = ["customer", "premium"],
        ShippingAddress = new Address { City = "New York", ZipCode = "10001" },
        Items = [.. Enumerable.Range(1, 50)
            .Select(i => new OrderItem { Sku = $"SKU{i:000}", Quantity = i, Price = i * 1.5m })]
    };

    public static WideModel ValidWideModel => new()
    {
        F1 = "Value 1",
        F2 = "Value 2",
        F3 = "Value 3",
        F4 = "Value 4",
        F5 = "Value 5",
        F6 = "Value 6",
        F7 = "Value 7",
        F8 = "Value 8",
        F9 = "Value 9",
        F10 = "Value 10",
        F11 = "Value 11",
        F12 = "Value 12"
    };

    public static WideModel InvalidWideModel => new()
    {
        F1 = "",
        F2 = "",
        F3 = "",
        F4 = "",
        F5 = "",
        F6 = "",
        F7 = "",
        F8 = "",
        F9 = "",
        F10 = "",
        F11 = "",
        F12 = ""
    };

    public static DeepOrder ValidDeepOrder => new()
    {
        CustomerName = "John Doe",
        ShippingAddress = new DeepAddress
        {
            City = "New York",
            Street = new Street { Line1 = "123 Main St", PostalCode = "10001" }
        }
    };

    public static Product ValidProduct => new()
    {
        Code = "PRD001",
        Status = "active",
        Score = 75,
        Price = 99.99m
    };

    public static Product InvalidProduct => new()
    {
        Code = null,
        Status = "banned",
        Score = null,
        Price = null
    };
}

// =========================================================================
// 1. Simple Validation (User model — NotEmpty / EmailAddress / GreaterThan / collection)
// =========================================================================

[MemoryDiagnoser]
public class SimpleValidationBenchmarks
{
    private readonly CseUserValidator _cse = new();
    private readonly FvUserValidator _fv = new();
    private User _valid = null!;
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidUser;
        _invalid = TestData.InvalidUser;
    }

    [Benchmark(Description = "CSE-Simple-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Simple-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-Simple-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Simple-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 2. String Validation (MinLength / MaxLength / Matches / Must)
// =========================================================================

[MemoryDiagnoser]
public class StringValidationBenchmarks
{
    private readonly CseStringValidator _cse = new();
    private readonly FvStringValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-String")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-String")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 3. Comparable Validation (GreaterThan / LessThan / InclusiveBetween)
// =========================================================================

[MemoryDiagnoser]
public class ComparableValidationBenchmarks
{
    private readonly CseComparableValidator _cse = new();
    private readonly FvComparableValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-Comparable")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Comparable")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 4. Collection Validation (NotEmpty / MinCount / MaxCount / CountBetween)
// =========================================================================

[MemoryDiagnoser]
public class CollectionValidationBenchmarks
{
    private readonly CseCollectionValidator _cse = new();
    private readonly FvCollectionValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-Collection")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Collection")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 5. Nested Object Validation (SetValidatorAsync — one level)
// =========================================================================

[MemoryDiagnoser]
public class NestedValidationBenchmarks
{
    private readonly CseNestedValidator _cse = new();
    private readonly FvNestedValidator _fv = new();
    private Order _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidOrder;

    [Benchmark(Description = "CSE-Nested")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Nested")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 6. Collection Item Validation (ForEach / RuleForEach)
// =========================================================================

[MemoryDiagnoser]
public class CollectionItemValidationBenchmarks
{
    private readonly CseCollectionItemValidator _cse = new();
    private readonly FvCollectionItemValidator _fv = new();
    private Order _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidOrder;

    [Benchmark(Description = "CSE-CollectionItem")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-CollectionItem")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 7. Conditional Validation (if / .When)
// =========================================================================

[MemoryDiagnoser]
public class ConditionalValidationBenchmarks
{
    private readonly CseConditionalValidator _cse = new();
    private readonly FvConditionalValidator _fv = new();
    private Order _business = null!;
    private Order _personal = null!;

    [GlobalSetup]
    public void Setup()
    {
        _business = TestData.BusinessOrder;
        _personal = TestData.PersonalOrder;
    }

    [Benchmark(Description = "CSE-Conditional-Business")]
    public object CseBusiness() => _cse.ValidateAsync(_business).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Conditional-Business")]
    public object FvBusiness() => _fv.Validate(_business);

    [Benchmark(Description = "CSE-Conditional-Personal")]
    public object CsePersonal() => _cse.ValidateAsync(_personal).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Conditional-Personal")]
    public object FvPersonal() => _fv.Validate(_personal);
}

// =========================================================================
// 8. Custom Predicate (Must — sync)
// =========================================================================

[MemoryDiagnoser]
public class CustomPredicateBenchmarks
{
    private readonly CseCustomPredicateValidator _cse = new();
    private readonly FvCustomPredicateValidator _fv = new();
    private User _valid = null!;
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidUser;
        _invalid = TestData.AdminUser;
    }

    [Benchmark(Description = "CSE-Must-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Must-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-Must-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Must-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 9. Async Predicate (MustAsync)
// =========================================================================

[MemoryDiagnoser]
public class AsyncPredicateBenchmarks
{
    private readonly CseAsyncPredicateValidator _cse = new();
    private readonly FvAsyncPredicateValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-MustAsync")]
    public async Task<object> CseAsync() => await _cse.ValidateAsync(_valid);

    [Benchmark(Description = "FV-MustAsync")]
    public async Task<object> FvAsync() => await _fv.ValidateAsync(_valid);
}

// =========================================================================
// 10. Cascade Continue (accumulate all errors)
// =========================================================================

[MemoryDiagnoser]
public class CascadeContinueBenchmarks
{
    private readonly CseCascadeContinueValidator _cse = new();
    private readonly FvCascadeContinueValidator _fv = new();
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup() => _invalid = TestData.WeakPasswordUser;

    [Benchmark(Description = "CSE-Cascade-Continue")]
    public object Cse() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Cascade-Continue")]
    public object Fv() => _fv.Validate(_invalid);
}

// =========================================================================
// 11. Complex Model Validation (all rule types)
// =========================================================================

[MemoryDiagnoser]
public class ComplexValidationBenchmarks
{
    private readonly CseComplexValidator _cse = new();
    private readonly FvComplexValidator _fv = new();
    private Order _valid = null!;
    private Order _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidOrder;
        _invalid = TestData.InvalidOrder;
    }

    [Benchmark(Description = "CSE-Complex-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Complex-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-Complex-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Complex-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 12. String Length Range (Length(min, max))
// =========================================================================

[MemoryDiagnoser]
public class StringLengthRangeBenchmarks
{
    private readonly CseStringLengthRangeValidator _cse = new();
    private readonly FvStringLengthRangeValidator _fv = new();
    private User _valid = null!;
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidUser;
        _invalid = TestData.InvalidUser;
    }

    [Benchmark(Description = "CSE-LengthRange-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-LengthRange-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-LengthRange-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-LengthRange-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 13. String Content (Contains / StartsWith / EndsWith)
//     CSE has built-in validators; FV uses Must() lambdas
// =========================================================================

[MemoryDiagnoser]
public class StringContentBenchmarks
{
    private readonly CseStringContentValidator _cse = new();
    private readonly FvStringContentValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-StringContent")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-StringContent")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 14. ExclusiveBetween
// =========================================================================

[MemoryDiagnoser]
public class ExclusiveBetweenBenchmarks
{
    private readonly CseExclusiveBetweenValidator _cse = new();
    private readonly FvExclusiveBetweenValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUser;

    [Benchmark(Description = "CSE-ExclusiveBetween")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-ExclusiveBetween")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 15. Nullable Value Type Validation (int? / decimal?)
// =========================================================================

[MemoryDiagnoser]
public class NullableValidationBenchmarks
{
    private readonly CseNullableValidator _cse = new();
    private readonly FvNullableValidator _fv = new();
    private Product _valid = null!;
    private Product _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidProduct;
        _invalid = TestData.InvalidProduct;
    }

    [Benchmark(Description = "CSE-Nullable-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Nullable-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-Nullable-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Nullable-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 16. String Equal / NotEqual
// =========================================================================

[MemoryDiagnoser]
public class StringEqualityBenchmarks
{
    private readonly CseStringEqualityValidator _cse = new();
    private readonly FvStringEqualityValidator _fv = new();
    private Product _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidProduct;

    [Benchmark(Description = "CSE-Equality")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Equality")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 17. Large Collection (ForEach / RuleForEach — 50 items)
// =========================================================================

[MemoryDiagnoser]
public class LargeCollectionBenchmarks
{
    private readonly CseLargeCollectionValidator _cse = new();
    private readonly FvLargeCollectionValidator _fv = new();
    private Order _large = null!;

    [GlobalSetup]
    public void Setup() => _large = TestData.LargeOrder;

    [Benchmark(Description = "CSE-LargeCollection")]
    public object Cse() => _cse.ValidateAsync(_large).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-LargeCollection")]
    public object Fv() => _fv.Validate(_large);
}

// =========================================================================
// 18. Wide Model (12 string fields)
// =========================================================================

[MemoryDiagnoser]
public class WideModelBenchmarks
{
    private readonly CseWideModelValidator _cse = new();
    private readonly FvWideModelValidator _fv = new();
    private WideModel _valid = null!;
    private WideModel _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidWideModel;
        _invalid = TestData.InvalidWideModel;
    }

    [Benchmark(Description = "CSE-Wide-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Wide-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-Wide-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Wide-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 19. Deep Nested Validation (3 levels: DeepOrder → DeepAddress → Street)
// =========================================================================

[MemoryDiagnoser]
public class DeepNestedBenchmarks
{
    private readonly CseDeepNestedValidator _cse = new();
    private readonly FvDeepNestedValidator _fv = new();
    private DeepOrder _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidDeepOrder;

    [Benchmark(Description = "CSE-DeepNested")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-DeepNested")]
    public object Fv() => _fv.Validate(_valid);
}

// =========================================================================
// 20. Cascade Stop (default stop-on-first-failure behavior)
// =========================================================================

[MemoryDiagnoser]
public class CascadeStopBenchmarks
{
    private readonly CseCascadeStopValidator _cse = new();
    private readonly FvCascadeStopValidator _fv = new();
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup() => _invalid = TestData.WeakPasswordUser;

    [Benchmark(Description = "CSE-Cascade-Stop")]
    public object Cse() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Cascade-Stop")]
    public object Fv() => _fv.Validate(_invalid);
}

// =========================================================================
// 21. Validator Construction Cost (ctor allocation overhead)
//     CSE: lazy — Configure() runs at validate time
//     FV:  eager — rule tree built in constructor
// =========================================================================

[MemoryDiagnoser]
public class ValidatorConstructionBenchmarks
{
    [Benchmark(Description = "CSE-Ctor")]
    public object Cse() => new CseUserValidator();

    [Benchmark(Description = "FV-Ctor")]
    public object Fv() => new FvUserValidator();
}

// =========================================================================
// 22. Multiple Regex (4 Matches rules, Cascade Continue)
// =========================================================================

[MemoryDiagnoser]
public class MultipleRegexBenchmarks
{
    private readonly CseMultipleRegexValidator _cse = new();
    private readonly FvMultipleRegexValidator _fv = new();
    private User _valid = null!;
    private User _invalid = null!;

    [GlobalSetup]
    public void Setup()
    {
        _valid = TestData.ValidUserWithPassword;
        _invalid = TestData.WeakPasswordUser;
    }

    [Benchmark(Description = "CSE-MultiRegex-Valid")]
    public object CseValid() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-MultiRegex-Valid")]
    public object FvValid() => _fv.Validate(_valid);

    [Benchmark(Description = "CSE-MultiRegex-Invalid")]
    public object CseInvalid() => _cse.ValidateAsync(_invalid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-MultiRegex-Invalid")]
    public object FvInvalid() => _fv.Validate(_invalid);
}

// =========================================================================
// 23. Pre-compiled Regex (Matches(Regex) overload vs string pattern cache)
// =========================================================================

[MemoryDiagnoser]
public class PrecompiledRegexBenchmarks
{
    private readonly CsePrecompiledRegexValidator _cse = new();
    private readonly FvPrecompiledRegexValidator _fv = new();
    private User _valid = null!;

    [GlobalSetup]
    public void Setup() => _valid = TestData.ValidUserWithPassword;

    [Benchmark(Description = "CSE-Regex-Precompiled")]
    public object Cse() => _cse.ValidateAsync(_valid).AsTask().GetAwaiter().GetResult();

    [Benchmark(Description = "FV-Regex-Precompiled")]
    public object Fv() => _fv.Validate(_valid);
}
