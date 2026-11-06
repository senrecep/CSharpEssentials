using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using CseScan = CSharpEssentials.Validation.Extensions.ValidatorServiceCollectionExtensions;
using FvScan = FluentValidation.ServiceCollectionExtensions;

namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>
/// Warm assembly scan: a new <see cref="ServiceCollection"/> plus a scan of this benchmark assembly, which holds every CSE and
/// FluentValidation validator. Each row returns the descriptor count. Semantic difference: FluentValidation registers each
/// validator under its interface and its concrete type (two descriptors), CSE adds one per validator via <c>TryAddEnumerable</c>,
/// so the counts differ; the setup checks that each library registered every one of its own validators. Both scans use
/// reflection. See <see cref="AssemblyScanColdComparison"/> for the first scan in a fresh process.
/// </summary>
[MemoryDiagnoser]
public class AssemblyScanComparison
{
    private static readonly Assembly Scanned = typeof(SignupRequest).Assembly;

    [GlobalSetup]
    public void Setup()
    {
        Check(CseServices(), typeof(IValidator<>), typeof(Validator<>), "CSE");
        Check(FvServices(), typeof(FluentValidation.IValidator<>), typeof(FluentValidation.AbstractValidator<>), "FV");
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Validation/Readme.MD#di-registration
    [Benchmark(Baseline = true)]
    public int CSharpEssentials() => CseServices().Count;

    // https://docs.fluentvalidation.net/en/latest/di.html#automatic-registration
    [Benchmark]
    public int FluentValidation() => FvServices().Count;

    internal static IServiceCollection CseServices() => CseScan.AddValidatorsFromAssembly(new ServiceCollection(), Scanned);

    internal static IServiceCollection FvServices() => FvScan.AddValidatorsFromAssembly(new ServiceCollection(), Scanned);

    private static void Check(IServiceCollection services, Type serviceInterface, Type validatorBase, string library)
    {
        Type[] validators = [.. Scanned.GetTypes().Where(t => !t.IsAbstract && BaseModel(t, validatorBase) is not null)];
        if (validators.Length == 0)
            throw new InvalidOperationException($"Self-check failed in {nameof(AssemblyScanComparison)}: no {library} validators found.");
        foreach (Type validator in validators)
        {
            Type service = serviceInterface.MakeGenericType(BaseModel(validator, validatorBase)!);
            if (!services.Any(d => d.ServiceType == service && d.ImplementationType == validator))
                throw new InvalidOperationException($"Self-check failed in {nameof(AssemblyScanComparison)}: {library} did not register {validator.Name}.");
        }
    }

    private static Type? BaseModel(Type type, Type validatorBase)
    {
        for (Type? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == validatorBase)
                return current.GetGenericArguments()[0];
        }
        return null;
    }
}
