```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method       | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |------------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |    952.7 ns | 12.04 ns | 11.27 ns |  1.00 |    0.02 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  3,572.7 ns | 20.82 ns | 19.47 ns |  3.75 |    0.05 | 0.6599 | 0.0191 |  10.83 KB |        2.21 |
| CseRuntime   |  4,817.0 ns | 25.38 ns | 22.49 ns |  5.06 |    0.06 | 0.8087 | 0.0229 |  13.32 KB |        2.72 |
| Scrutor      | 11,877.3 ns | 41.02 ns | 38.37 ns | 12.47 |    0.15 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
