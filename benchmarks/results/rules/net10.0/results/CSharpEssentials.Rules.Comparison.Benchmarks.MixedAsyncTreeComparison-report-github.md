```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   976.8 ns | 16.30 ns | 15.25 ns |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,375.1 ns | 27.47 ns | 51.60 ns |  1.41 |    0.06 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync |   971.9 ns | 18.35 ns | 17.16 ns |  1.00 |    0.02 | 0.0343 |     600 B |        1.23 |
