```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  8.817 μs | 0.0764 μs | 0.1578 μs |  1.00 |    0.02 | 0.5493 |   9.53 KB |        1.00 |
| CSharpEssentials_SkipBodies | 10.010 μs | 0.1915 μs | 0.2422 μs |  1.14 |    0.03 | 0.6104 |  10.24 KB |        1.07 |
| Serilog_RequestLogging      | 11.871 μs | 0.2104 μs | 0.2808 μs |  1.35 |    0.04 | 0.7324 |  12.17 KB |        1.28 |
| HttpLogging_Metadata        |  9.795 μs | 0.1821 μs | 0.1789 μs |  1.11 |    0.03 | 0.6104 |  10.19 KB |        1.07 |
