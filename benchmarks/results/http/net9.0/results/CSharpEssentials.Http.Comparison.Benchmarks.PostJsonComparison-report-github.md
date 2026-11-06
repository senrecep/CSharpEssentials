```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                              | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_PostDefaultOptions | Defaults   |  3.092 μs | 0.0134 μs | 0.0112 μs |  1.00 |    0.00 | 0.1526 |    2.9 KB |        1.00 |
|                                     |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Post               | Post       |  3.041 μs | 0.0078 μs | 0.0073 μs |  1.00 |    0.00 | 0.1678 |   2.79 KB |        1.00 |
| Bcl_Post                            | Post       |  2.668 μs | 0.0040 μs | 0.0033 μs |  0.88 |    0.00 | 0.1450 |   2.42 KB |        0.87 |
| Refit_Post                          | Post       |  3.851 μs | 0.0103 μs | 0.0096 μs |  1.27 |    0.00 | 0.1984 |   3.34 KB |        1.20 |
| Flurl_Post                          | Post       |  6.301 μs | 0.0207 μs | 0.0161 μs |  2.07 |    0.01 | 0.3662 |   6.42 KB |        2.30 |
| RestSharp_Post                      | Post       | 14.542 μs | 0.1181 μs | 0.1104 μs |  4.78 |    0.04 | 1.8311 |  31.12 KB |       11.16 |
