```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             | 12.30 μs | 0.236 μs | 0.253 μs |  1.00 |    0.03 | 0.6104 |  11.51 KB |        1.00 |
| CSharpEssentials_SkipBodies | 12.86 μs | 0.254 μs | 0.330 μs |  1.05 |    0.03 | 0.7324 |  12.17 KB |        1.06 |
| Serilog_RequestLogging      | 16.89 μs | 0.305 μs | 0.270 μs |  1.37 |    0.03 | 0.8545 |  13.82 KB |        1.20 |
| HttpLogging_Metadata        | 13.22 μs | 0.237 μs | 0.317 μs |  1.08 |    0.03 | 0.7324 |  12.14 KB |        1.06 |
