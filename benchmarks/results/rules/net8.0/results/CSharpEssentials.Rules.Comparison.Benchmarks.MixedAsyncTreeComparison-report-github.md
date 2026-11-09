```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         | 1.014 μs | 0.0195 μs | 0.0209 μs | 1.019 μs |  1.00 |    0.03 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1.589 μs | 0.0317 μs | 0.0741 μs | 1.562 μs |  1.57 |    0.08 | 0.0381 |     670 B |        1.38 |
| CseEvaluateSyncOverAsync | 1.078 μs | 0.0202 μs | 0.0296 μs | 1.081 μs |  1.06 |    0.04 | 0.0343 |     600 B |        1.23 |
