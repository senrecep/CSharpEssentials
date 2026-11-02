```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |  1.187 μs | 0.0108 μs | 0.0095 μs |  1.00 |    0.01 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  4.377 μs | 0.0248 μs | 0.0232 μs |  3.69 |    0.03 | 0.6638 | 0.0153 |   10.9 KB |        2.23 |
| CseRuntime   |  5.601 μs | 0.0345 μs | 0.0323 μs |  4.72 |    0.05 | 0.8469 | 0.0229 |  13.95 KB |        2.85 |
| Scrutor      | 12.500 μs | 0.0451 μs | 0.0422 μs | 10.53 |    0.09 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
