namespace CSharpEssentials.Results.Comparison.Benchmarks;

public static class ResultsSelfCheck
{
    public static void VerifyAll()
    {
        bool[] flags = [true, false];
        new ErrorFactoryComparison().Setup();
        new ErrorToStringComparison().Setup();
        new ErrorJsonComparison().Setup();
        foreach (bool flag in flags)
        {
            new CreateComparison { Succeed = flag }.Setup();
            new MapComparison { Succeed = flag }.Setup();
            new BindComparison { Succeed = flag }.Setup();
            new EnsureComparison { Succeed = flag }.Setup();
            new MatchComparison { Succeed = flag }.Setup();
            new TapComparison { Succeed = flag }.Setup();
            new ErrorAggregationComparison { Succeed = flag }.Setup();
            new MaybeComparison { HasValue = flag }.Setup();
            new ErrorEqualityComparison { WithMetadata = flag }.Setup();
            new ErrorExceptionComparison { Thrown = flag }.Setup();
            new TryComparison { Succeed = flag }.Setup();
            foreach (bool yield in flags)
            {
                new AsyncPipelineComparison { Succeed = flag, Yield = yield }.Setup();
                new TryAsyncComparison { Succeed = flag, Yield = yield }.Setup();
            }

            foreach (int n in new[] { 10, 100 })
                new SequenceComparison { N = n, Succeed = flag }.Setup();
        }

        foreach (int entries in new[] { 1, 8, 32 })
            new ErrorMetadataToStringComparison { Entries = entries }.Setup();
        foreach (string source in new[] { MaybeCollectionComparison.ListSource, MaybeCollectionComparison.SequenceSource })
            new MaybeCollectionComparison { Source = source }.Setup();
    }
}
