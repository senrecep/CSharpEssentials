```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   970.7 ns | 11.98 ns | 11.20 ns |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,565.4 ns | 21.60 ns | 20.21 ns |  1.61 |    0.03 | 0.0572 |     983 B |        2.02 |
| CseEvaluateSyncOverAsync | 1,053.8 ns | 19.19 ns | 17.01 ns |  1.09 |    0.02 | 0.0343 |     600 B |        1.23 |
