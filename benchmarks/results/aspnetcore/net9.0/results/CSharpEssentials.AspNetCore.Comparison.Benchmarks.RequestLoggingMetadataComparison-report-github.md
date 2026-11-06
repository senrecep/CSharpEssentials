```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  9.461 μs | 0.1842 μs | 0.2048 μs |  1.00 |    0.03 | 0.5493 |   9.66 KB |        1.00 |
| CSharpEssentials_SkipBodies | 10.300 μs | 0.1055 μs | 0.0824 μs |  1.09 |    0.02 | 0.6104 |  10.43 KB |        1.08 |
| Serilog_RequestLogging      | 12.484 μs | 0.2460 μs | 0.2301 μs |  1.32 |    0.04 | 0.7324 |  12.39 KB |        1.28 |
| HttpLogging_Metadata        | 10.336 μs | 0.2002 μs | 0.2226 μs |  1.09 |    0.03 | 0.6104 |  10.31 KB |        1.07 |
