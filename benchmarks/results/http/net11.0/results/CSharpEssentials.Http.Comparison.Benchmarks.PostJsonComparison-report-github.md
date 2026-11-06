```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                              | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_PostDefaultOptions | Defaults   |  2.528 μs | 0.0125 μs | 0.0111 μs |  1.00 |    0.01 | 0.1526 |   2.91 KB |        1.00 |
|                                     |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Post               | Post       |  2.496 μs | 0.0125 μs | 0.0111 μs |  1.00 |    0.01 | 0.1526 |    2.8 KB |        1.00 |
| Bcl_Post                            | Post       |  2.213 μs | 0.0115 μs | 0.0102 μs |  0.89 |    0.01 | 0.1221 |   2.45 KB |        0.87 |
| Refit_Post                          | Post       |  3.184 μs | 0.0121 μs | 0.0107 μs |  1.28 |    0.01 | 0.1831 |   3.36 KB |        1.20 |
| Flurl_Post                          | Post       |  5.121 μs | 0.0800 μs | 0.0624 μs |  2.05 |    0.03 | 0.3662 |   6.56 KB |        2.34 |
| RestSharp_Post                      | Post       | 10.878 μs | 0.1825 μs | 0.1618 μs |  4.36 |    0.07 | 1.8311 |  30.75 KB |       10.96 |
