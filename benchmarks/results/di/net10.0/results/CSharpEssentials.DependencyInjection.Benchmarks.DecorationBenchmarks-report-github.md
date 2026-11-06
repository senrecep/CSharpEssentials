```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method       | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| Manual       |  1.010 μs | 0.0142 μs | 0.0132 μs |  1.00 |    0.02 | 0.2995 | 0.0038 |    4.9 KB |        1.00 |
| CseGenerated |  2.137 μs | 0.0120 μs | 0.0112 μs |  2.12 |    0.03 | 0.4311 | 0.0076 |   7.09 KB |        1.45 |
| CseRuntime   |  3.732 μs | 0.0236 μs | 0.0221 μs |  3.70 |    0.05 | 0.6714 | 0.0191 |  10.99 KB |        2.24 |
| Scrutor      | 12.217 μs | 0.0265 μs | 0.0235 μs | 12.10 |    0.15 | 0.6866 | 0.0153 |  11.46 KB |        2.34 |
