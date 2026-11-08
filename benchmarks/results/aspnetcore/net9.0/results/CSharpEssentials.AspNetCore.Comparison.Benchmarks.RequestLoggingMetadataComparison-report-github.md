```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  8.558 μs | 0.1379 μs | 0.1290 μs |  1.00 |    0.02 | 0.5493 |   9.66 KB |        1.00 |
| CSharpEssentials_SkipBodies |  9.483 μs | 0.1813 μs | 0.2713 μs |  1.11 |    0.04 | 0.6104 |  10.43 KB |        1.08 |
| Serilog_RequestLogging      | 11.090 μs | 0.1840 μs | 0.2327 μs |  1.30 |    0.03 | 0.7324 |  12.41 KB |        1.29 |
| HttpLogging_Metadata        |  9.472 μs | 0.1870 μs | 0.2497 μs |  1.11 |    0.03 | 0.6104 |   10.3 KB |        1.07 |
