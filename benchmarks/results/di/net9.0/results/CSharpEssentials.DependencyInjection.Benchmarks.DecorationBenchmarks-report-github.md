```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |  1.239 μs | 0.0110 μs | 0.0103 μs |  1.00 |    0.01 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  2.644 μs | 0.0108 μs | 0.0096 μs |  2.13 |    0.02 | 0.4349 | 0.0076 |   7.16 KB |        1.46 |
| CseRuntime   |  4.515 μs | 0.0233 μs | 0.0218 μs |  3.65 |    0.03 | 0.6714 | 0.0153 |  10.99 KB |        2.24 |
| Scrutor      | 12.647 μs | 0.0388 μs | 0.0363 μs | 10.21 |    0.09 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
