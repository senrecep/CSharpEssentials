```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |   725.6 ns |   4.21 ns |   3.94 ns |  1.00 |    0.01 | 0.0963 |   1.59 KB |        1.00 |
| Bcl_NotFound              |   621.4 ns |   3.57 ns |   3.34 ns |  0.86 |    0.01 | 0.0830 |   1.36 KB |        0.86 |
| Refit_NotFound            | 2,670.1 ns |  16.73 ns |  13.06 ns |  3.68 |    0.03 | 0.2136 |   3.91 KB |        2.47 |
| Flurl_NotFound            | 3,316.8 ns |   8.31 ns |   7.78 ns |  4.57 |    0.03 | 0.2975 |   4.89 KB |        3.08 |
| RestSharp_NotFound        | 9,125.7 ns | 179.31 ns | 199.30 ns | 12.58 |    0.28 | 1.8311 |  30.08 KB |       18.97 |
