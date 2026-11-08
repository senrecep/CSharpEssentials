```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  9.152 μs | 0.1594 μs | 0.1331 μs |  1.00 |    0.02 | 0.5493 |   9.71 KB |        1.00 |
| CSharpEssentials_SkipBodies |  9.977 μs | 0.1782 μs | 0.1830 μs |  1.09 |    0.02 | 0.6104 |  10.49 KB |        1.08 |
| Serilog_RequestLogging      | 12.052 μs | 0.2173 μs | 0.3185 μs |  1.32 |    0.04 | 0.7324 |  12.49 KB |        1.29 |
| HttpLogging_Metadata        |  9.752 μs | 0.1910 μs | 0.1787 μs |  1.07 |    0.02 | 0.6104 |  10.36 KB |        1.07 |
