```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         | 1.007 μs | 0.0201 μs | 0.0215 μs |  1.00 |    0.03 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1.558 μs | 0.0311 μs | 0.0493 μs |  1.55 |    0.06 | 0.0381 |     670 B |        1.38 |
| CseEvaluateSyncOverAsync | 1.092 μs | 0.0218 μs | 0.0479 μs |  1.08 |    0.05 | 0.0343 |     600 B |        1.23 |
