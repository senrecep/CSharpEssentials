```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                   | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| HandWrittenAsync         |   790.4 ns |  5.49 ns |  4.86 ns |  1.00 |    0.01 | 0.0267 |     486 B |        1.00 |
| CseEvaluateAsync         | 1,256.1 ns | 10.64 ns |  8.88 ns |  1.59 |    0.01 | 0.0381 |     667 B |        1.37 |
| CseEvaluateSyncOverAsync |   936.0 ns | 11.99 ns | 11.22 ns |  1.18 |    0.02 | 0.0343 |     591 B |        1.22 |
