```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   985.4 ns | 13.70 ns | 12.82 ns |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,493.8 ns | 24.17 ns | 22.61 ns |  1.52 |    0.03 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync | 1,197.6 ns | 22.52 ns | 21.06 ns |  1.22 |    0.03 | 0.0343 |     600 B |        1.23 |
