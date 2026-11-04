namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Runs every class's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class ValidationSelfCheck
{
    public static void VerifyAll()
    {
        new SimpleValidationBenchmarks().Setup();
        new StringValidationBenchmarks().Setup();
        new ComparableValidationBenchmarks().Setup();
        new CollectionValidationBenchmarks().Setup();
        new NestedValidationBenchmarks().Setup();
        new CollectionItemValidationBenchmarks().Setup();
        new ConditionalValidationBenchmarks().Setup();
        new CustomPredicateBenchmarks().Setup();
        new AsyncPredicateBenchmarks().Setup();
        new CascadeContinueBenchmarks().Setup();
        new ComplexValidationBenchmarks().Setup();
        new StringLengthRangeBenchmarks().Setup();
        new StringContentBenchmarks().Setup();
        new ExclusiveBetweenBenchmarks().Setup();
        new NullableValidationBenchmarks().Setup();
        new StringEqualityBenchmarks().Setup();
        new LargeCollectionBenchmarks().Setup();
        new WideModelBenchmarks().Setup();
        new DeepNestedBenchmarks().Setup();
        new CascadeStopBenchmarks().Setup();
        new ValidatorConstructionBenchmarks().Setup();
        new MultipleRegexBenchmarks().Setup();
        new PrecompiledRegexBenchmarks().Setup();
        new AssemblyScanComparison().Setup();
        foreach (bool valid in new[] { true, false })
        {
            new BaselineSimpleComparison { Valid = valid }.Setup();
            new BaselineWideComparison { Valid = valid }.Setup();
            new ForOverloadComparison { Valid = valid }.Setup();
            new EnumFlagsComparison { Valid = valid }.Setup();
            new EnumOneOfComparison { Valid = valid }.Setup();
            foreach (MetadataKind metadata in new[] { MetadataKind.Plain, MetadataKind.Generated })
                new EnumDefinedComparison { Metadata = metadata, Valid = valid }.Setup();
        }
    }
}
