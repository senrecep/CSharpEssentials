```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method       | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |   952.9 ns | 11.47 ns | 10.73 ns |  1.00 |    0.02 | 0.2995 | 0.0048 |    4.9 KB |        1.00 |
| CseGenerated | 2,144.3 ns | 42.84 ns | 40.07 ns |  2.25 |    0.05 | 0.4272 | 0.0076 |   7.02 KB |        1.43 |
| CseRuntime   | 3,512.7 ns | 31.81 ns | 29.75 ns |  3.69 |    0.05 | 0.6561 | 0.0191 |  10.76 KB |        2.20 |
| Scrutor      | 4,253.0 ns | 37.30 ns | 34.89 ns |  4.46 |    0.06 | 0.6866 | 0.0153 |  11.31 KB |        2.31 |
