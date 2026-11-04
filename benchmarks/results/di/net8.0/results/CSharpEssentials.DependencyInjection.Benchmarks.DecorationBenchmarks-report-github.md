```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |  1.148 μs | 0.0138 μs | 0.0129 μs |  1.00 |    0.02 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  4.349 μs | 0.0315 μs | 0.0295 μs |  3.79 |    0.05 | 0.6638 | 0.0153 |   10.9 KB |        2.23 |
| CseRuntime   |  6.025 μs | 0.0181 μs | 0.0160 μs |  5.25 |    0.06 | 0.8698 | 0.0229 |  14.26 KB |        2.91 |
| Scrutor      | 13.108 μs | 0.0609 μs | 0.0570 μs | 11.42 |    0.13 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
