```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   990.7 ns |  8.28 ns |  7.34 ns |  1.00 |    0.01 | 0.0286 |     486 B |        1.00 |
| CseEvaluateAsync         | 1,686.4 ns | 31.64 ns | 32.49 ns |  1.70 |    0.03 | 0.0572 |     983 B |        2.02 |
| CseEvaluateSyncOverAsync | 1,128.0 ns | 21.19 ns | 26.79 ns |  1.14 |    0.03 | 0.0343 |     600 B |        1.23 |
