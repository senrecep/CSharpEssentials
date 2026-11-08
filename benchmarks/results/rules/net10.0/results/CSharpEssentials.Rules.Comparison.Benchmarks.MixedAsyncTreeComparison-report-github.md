```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   934.0 ns |  8.36 ns |  6.98 ns |  1.00 |    0.01 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,308.2 ns | 21.39 ns | 20.00 ns |  1.40 |    0.02 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync | 1,096.1 ns | 21.65 ns | 21.26 ns |  1.17 |    0.02 | 0.0343 |     600 B |        1.23 |
