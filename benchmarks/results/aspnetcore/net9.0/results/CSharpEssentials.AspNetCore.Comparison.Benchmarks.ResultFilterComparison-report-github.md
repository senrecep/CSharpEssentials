```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Result  

```
| Method                        | Outcome | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |-------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_TypedResults**              | **Failure** | **11.46 μs** | **0.212 μs** | **0.177 μs** |  **1.00** |    **0.02** | **0.6104** |  **10.44 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Failure | 14.32 μs | 0.279 μs | 0.391 μs |  1.25 |    0.04 | 0.6104 |  11.53 KB |        1.10 |
| CSharpEssentials_ManualMatch  | Failure | 14.03 μs | 0.270 μs | 0.379 μs |  1.22 |    0.04 | 0.6104 |  11.46 KB |        1.10 |
|                               |         |          |          |          |       |         |        |           |             |
| **Bcl_TypedResults**              | **Success** | **10.23 μs** | **0.194 μs** | **0.172 μs** |  **1.00** |    **0.02** | **0.4883** |      **9 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Success | 10.43 μs | 0.170 μs | 0.215 μs |  1.02 |    0.03 | 0.4883 |   9.07 KB |        1.01 |
| CSharpEssentials_ManualMatch  | Success | 10.17 μs | 0.201 μs | 0.377 μs |  0.99 |    0.04 | 0.4883 |   8.98 KB |        1.00 |
