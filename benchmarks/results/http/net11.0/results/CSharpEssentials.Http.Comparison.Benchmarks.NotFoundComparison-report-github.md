```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                    | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_NotFound |   691.9 ns |   3.73 ns |   3.49 ns |  1.00 |    0.01 | 0.0963 |      - |   1.59 KB |        1.00 |
| Bcl_NotFound              |   579.0 ns |   4.41 ns |   3.91 ns |  0.84 |    0.01 | 0.0830 |      - |   1.37 KB |        0.86 |
| Refit_NotFound            | 2,581.0 ns |  19.66 ns |  18.39 ns |  3.73 |    0.03 | 0.2136 |      - |   3.92 KB |        2.47 |
| Flurl_NotFound            | 3,178.9 ns |  15.36 ns |  13.62 ns |  4.59 |    0.03 | 0.3052 |      - |   5.04 KB |        3.18 |
| RestSharp_NotFound        | 8,629.2 ns | 171.06 ns | 197.00 ns | 12.47 |    0.28 | 1.8311 | 0.1221 |  30.06 KB |       18.95 |
