```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Result  

```
| Method                        | Outcome | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |-------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_TypedResults**              | **Failure** | **11.58 μs** | **0.229 μs** | **0.376 μs** |  **1.00** |    **0.05** | **0.4883** |   **9.61 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Failure | 14.95 μs | 0.236 μs | 0.263 μs |  1.29 |    0.05 | 0.6104 |  11.64 KB |        1.21 |
| CSharpEssentials_ManualMatch  | Failure | 14.42 μs | 0.287 μs | 0.447 μs |  1.25 |    0.05 | 0.6104 |  11.57 KB |        1.20 |
|                               |         |          |          |          |       |         |        |           |             |
| **Bcl_TypedResults**              | **Success** | **11.00 μs** | **0.214 μs** | **0.364 μs** |  **1.00** |    **0.05** | **0.4883** |   **9.11 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Success | 11.26 μs | 0.220 μs | 0.397 μs |  1.02 |    0.05 | 0.4883 |   9.18 KB |        1.01 |
| CSharpEssentials_ManualMatch  | Success | 10.84 μs | 0.203 μs | 0.199 μs |  0.99 |    0.04 | 0.4883 |   9.09 KB |        1.00 |
