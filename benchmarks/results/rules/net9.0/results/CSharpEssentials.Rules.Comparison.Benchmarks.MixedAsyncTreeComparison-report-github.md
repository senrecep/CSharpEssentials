```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   995.8 ns | 18.62 ns | 17.42 ns |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,592.4 ns | 31.37 ns | 48.83 ns |  1.60 |    0.06 | 0.0381 |     668 B |        1.37 |
| CseEvaluateSyncOverAsync | 1,047.5 ns | 20.65 ns | 39.28 ns |  1.05 |    0.04 | 0.0343 |     600 B |        1.23 |
