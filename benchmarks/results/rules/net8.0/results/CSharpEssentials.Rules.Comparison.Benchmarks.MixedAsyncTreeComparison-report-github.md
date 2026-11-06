```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         | 1.012 μs | 0.0090 μs | 0.0084 μs |  1.00 |    0.01 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1.859 μs | 0.0282 μs | 0.0264 μs |  1.84 |    0.03 | 0.0591 |     992 B |        2.04 |
| CseEvaluateSyncOverAsync | 1.200 μs | 0.0111 μs | 0.0098 μs |  1.19 |    0.01 | 0.0343 |     600 B |        1.23 |
