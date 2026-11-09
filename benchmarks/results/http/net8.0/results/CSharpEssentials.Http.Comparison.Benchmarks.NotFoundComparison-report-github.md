```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                    | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |    965.0 ns |   6.41 ns |   5.69 ns |  1.00 |    0.01 | 0.0954 |   1.58 KB |        1.00 |
| Bcl_NotFound              |    743.9 ns |   2.79 ns |   2.47 ns |  0.77 |    0.01 | 0.0820 |   1.35 KB |        0.86 |
| Refit_NotFound            |  3,501.7 ns |  16.90 ns |  15.81 ns |  3.63 |    0.03 | 0.2365 |   3.92 KB |        2.49 |
| Flurl_NotFound            |  4,253.5 ns |  20.33 ns |  19.02 ns |  4.41 |    0.03 | 0.2975 |   4.88 KB |        3.09 |
| RestSharp_NotFound        | 11,778.9 ns | 143.96 ns | 134.66 ns | 12.21 |    0.15 | 1.8311 |  30.47 KB |       19.31 |
