```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2

Categories=Metadata  

```
| Method                      | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Plain_NoLogging             | 10.69 μs | 0.203 μs | 0.445 μs |  1.00 |    0.06 | 0.7324 |  12.33 KB |        1.00 |
| CSharpEssentials_SkipBodies | 11.47 μs | 0.226 μs | 0.514 μs |  1.07 |    0.06 | 0.7324 |  13.06 KB |        1.06 |
| Serilog_RequestLogging      | 12.56 μs | 0.250 μs | 0.417 μs |  1.18 |    0.06 | 0.8545 |     15 KB |        1.22 |
| HttpLogging_Metadata        | 11.78 μs | 0.232 μs | 0.490 μs |  1.10 |    0.06 | 0.7324 |  13.05 KB |        1.06 |
