```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   968.3 ns | 18.23 ns | 18.72 ns |  1.00 |    0.03 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,395.1 ns | 27.68 ns | 47.74 ns |  1.44 |    0.06 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync | 1,003.4 ns | 18.74 ns | 31.82 ns |  1.04 |    0.04 | 0.0343 |     600 B |        1.23 |
