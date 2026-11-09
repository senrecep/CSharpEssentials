```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   794.3 ns |  5.06 ns |  4.73 ns |  1.00 |    0.01 | 0.0286 |     485 B |        1.00 |
| CseEvaluateAsync         | 1,255.7 ns |  5.41 ns |  5.06 ns |  1.58 |    0.01 | 0.0381 |     669 B |        1.38 |
| CseEvaluateSyncOverAsync |   825.2 ns | 12.45 ns | 12.23 ns |  1.04 |    0.02 | 0.0353 |     591 B |        1.22 |
