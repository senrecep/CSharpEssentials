```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                              | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| CSharpEssentials_PostDefaultOptions | Defaults   |  2.642 μs | 0.0192 μs | 0.0170 μs |  1.00 |    0.01 | 0.1526 |   2.92 KB |        1.00 |
|                                     |            |           |           |           |       |         |        |           |             |
| CSharpEssentials_Post               | Post       |  2.598 μs | 0.0103 μs | 0.0080 μs |  1.00 |    0.00 | 0.1526 |   2.81 KB |        1.00 |
| Bcl_Post                            | Post       |  2.336 μs | 0.0082 μs | 0.0077 μs |  0.90 |    0.00 | 0.1373 |   2.45 KB |        0.87 |
| Refit_Post                          | Post       |  3.406 μs | 0.0104 μs | 0.0093 μs |  1.31 |    0.01 | 0.1831 |   3.36 KB |        1.19 |
| Flurl_Post                          | Post       |  5.260 μs | 0.0268 μs | 0.0238 μs |  2.02 |    0.01 | 0.3662 |   6.43 KB |        2.29 |
| RestSharp_Post                      | Post       | 12.006 μs | 0.1049 μs | 0.0981 μs |  4.62 |    0.04 | 1.8311 |  30.77 KB |       10.94 |
