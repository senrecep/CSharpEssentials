```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |  1.200 μs | 0.0085 μs | 0.0079 μs |  1.00 |    0.01 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  2.583 μs | 0.0221 μs | 0.0207 μs |  2.15 |    0.02 | 0.4349 | 0.0076 |   7.16 KB |        1.46 |
| CseRuntime   |  4.721 μs | 0.0228 μs | 0.0213 μs |  3.94 |    0.03 | 0.6714 | 0.0153 |  10.99 KB |        2.24 |
| Scrutor      | 13.315 μs | 0.0232 μs | 0.0205 μs | 11.10 |    0.07 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
