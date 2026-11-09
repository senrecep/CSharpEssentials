```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                   | Mean     | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |---------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         | 1.003 μs | 0.0153 μs | 0.0143 μs |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1.445 μs | 0.0268 μs | 0.0237 μs |  1.44 |    0.03 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync | 1.034 μs | 0.0201 μs | 0.0223 μs |  1.03 |    0.03 | 0.0343 |     600 B |        1.23 |
