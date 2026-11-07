```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   983.2 ns | 13.81 ns | 12.24 ns |  1.00 |    0.02 | 0.0286 |     487 B |        1.00 |
| CseEvaluateAsync         | 1,410.0 ns | 27.37 ns | 29.28 ns |  1.43 |    0.03 | 0.0401 |     671 B |        1.38 |
| CseEvaluateSyncOverAsync | 1,077.4 ns | 20.48 ns | 19.16 ns |  1.10 |    0.02 | 0.0343 |     600 B |        1.23 |
