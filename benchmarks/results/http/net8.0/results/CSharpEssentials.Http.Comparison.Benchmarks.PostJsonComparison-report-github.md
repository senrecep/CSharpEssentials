```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                              | Categories | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------------ |----------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| CSharpEssentials_PostDefaultOptions | Defaults   |  3.264 μs | 0.0338 μs | 0.0299 μs |  1.00 |    0.01 | 0.1755 |      - |    2.9 KB |        1.00 |
|                                     |            |           |           |           |       |         |        |        |           |             |
| CSharpEssentials_Post               | Post       |  3.400 μs | 0.0250 μs | 0.0234 μs |  1.00 |    0.01 | 0.1678 |      - |   2.79 KB |        1.00 |
| Bcl_Post                            | Post       |  2.864 μs | 0.0411 μs | 0.0384 μs |  0.84 |    0.01 | 0.1450 |      - |   2.42 KB |        0.87 |
| Refit_Post                          | Post       |  4.264 μs | 0.0091 μs | 0.0071 μs |  1.25 |    0.01 | 0.1984 |      - |   3.34 KB |        1.20 |
| Flurl_Post                          | Post       |  6.884 μs | 0.0149 μs | 0.0132 μs |  2.03 |    0.01 | 0.3967 |      - |   6.58 KB |        2.36 |
| RestSharp_Post                      | Post       | 15.156 μs | 0.1851 μs | 0.1641 μs |  4.46 |    0.06 | 1.8311 | 0.1221 |  31.38 KB |       11.25 |
