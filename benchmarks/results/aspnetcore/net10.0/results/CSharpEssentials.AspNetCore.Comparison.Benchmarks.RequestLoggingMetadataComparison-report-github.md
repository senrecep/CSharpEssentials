```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  8.255 μs | 0.1232 μs | 0.1153 μs |  1.00 |    0.02 | 0.5493 |   9.53 KB |        1.00 |
| CSharpEssentials_SkipBodies |  8.724 μs | 0.1632 μs | 0.1527 μs |  1.06 |    0.02 | 0.6104 |  10.22 KB |        1.07 |
| Serilog_RequestLogging      | 10.866 μs | 0.2128 μs | 0.2913 μs |  1.32 |    0.04 | 0.7324 |  12.22 KB |        1.28 |
| HttpLogging_Metadata        |  8.918 μs | 0.1589 μs | 0.1486 μs |  1.08 |    0.02 | 0.6104 |  10.17 KB |        1.07 |
