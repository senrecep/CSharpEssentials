```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             |  9.774 μs | 0.1940 μs | 0.2156 μs |  1.00 |    0.03 | 0.5493 |   9.71 KB |        1.00 |
| CSharpEssentials_SkipBodies | 10.813 μs | 0.2116 μs | 0.1875 μs |  1.11 |    0.03 | 0.6104 |  10.51 KB |        1.08 |
| Serilog_RequestLogging      | 13.151 μs | 0.2556 μs | 0.2391 μs |  1.35 |    0.04 | 0.7324 |  12.51 KB |        1.29 |
| HttpLogging_Metadata        | 10.878 μs | 0.2142 μs | 0.2859 μs |  1.11 |    0.04 | 0.6104 |  10.36 KB |        1.07 |
