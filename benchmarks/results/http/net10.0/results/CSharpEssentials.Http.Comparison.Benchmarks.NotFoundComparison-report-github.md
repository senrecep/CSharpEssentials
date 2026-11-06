```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |   879.5 ns |   3.99 ns |   3.74 ns |  1.00 |    0.01 | 0.1068 |   1.75 KB |        1.00 |
| Bcl_NotFound              |   635.4 ns |   3.09 ns |   2.89 ns |  0.72 |    0.00 | 0.0830 |   1.36 KB |        0.78 |
| Refit_NotFound            | 2,623.9 ns |  23.87 ns |  22.33 ns |  2.98 |    0.03 | 0.2136 |   3.91 KB |        2.24 |
| Flurl_NotFound            | 3,329.0 ns |   7.82 ns |   6.93 ns |  3.78 |    0.02 | 0.2975 |   4.89 KB |        2.79 |
| RestSharp_NotFound        | 9,167.7 ns | 170.42 ns | 209.29 ns | 10.42 |    0.24 | 1.8311 |  30.08 KB |       17.18 |
