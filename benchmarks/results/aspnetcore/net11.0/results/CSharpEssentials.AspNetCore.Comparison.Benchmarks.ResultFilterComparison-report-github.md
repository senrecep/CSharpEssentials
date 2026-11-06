```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Result  

```
| Method                        | Outcome | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |-------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| **Bcl_TypedResults**              | **Failure** | **12.72 μs** | **0.249 μs** | **0.402 μs** |  **1.00** |    **0.04** | **0.6104** |      **-** |  **11.61 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Failure | 14.88 μs | 0.295 μs | 0.501 μs |  1.17 |    0.05 | 0.7324 | 0.7324 |  12.77 KB |        1.10 |
| CSharpEssentials_ManualMatch  | Failure | 14.68 μs | 0.290 μs | 0.630 μs |  1.16 |    0.06 | 0.7324 |      - |  12.71 KB |        1.10 |
|                               |         |          |          |          |       |         |        |        |           |             |
| **Bcl_TypedResults**              | **Success** | **13.27 μs** | **0.242 μs** | **0.411 μs** |  **1.00** |    **0.04** | **0.6104** |      **-** |  **10.09 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Success | 13.09 μs | 0.255 μs | 0.381 μs |  0.99 |    0.04 | 0.6104 |      - |  10.17 KB |        1.01 |
| CSharpEssentials_ManualMatch  | Success | 13.09 μs | 0.255 μs | 0.357 μs |  0.99 |    0.04 | 0.6104 |      - |  10.07 KB |        1.00 |
