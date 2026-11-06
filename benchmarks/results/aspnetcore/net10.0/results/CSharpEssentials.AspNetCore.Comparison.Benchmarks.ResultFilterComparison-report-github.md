```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Result  

```
| Method                        | Outcome | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------ |-------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_TypedResults**              | **Failure** | **11.021 μs** | **0.2151 μs** | **0.3085 μs** |  **1.00** |    **0.04** | **0.6104** |  **10.19 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Failure | 13.547 μs | 0.2668 μs | 0.3176 μs |  1.23 |    0.04 | 0.6104 |  11.35 KB |        1.11 |
| CSharpEssentials_ManualMatch  | Failure | 12.940 μs | 0.2586 μs | 0.3540 μs |  1.18 |    0.04 | 0.6104 |  11.29 KB |        1.11 |
|                               |         |           |           |           |       |         |        |           |             |
| **Bcl_TypedResults**              | **Success** |  **9.475 μs** | **0.1687 μs** | **0.2472 μs** |  **1.00** |    **0.04** | **0.4883** |   **8.67 KB** |        **1.00** |
| CSharpEssentials_ResultFilter | Success | 11.231 μs | 0.2180 μs | 0.2039 μs |  1.19 |    0.04 | 0.4883 |   8.74 KB |        1.01 |
| CSharpEssentials_ManualMatch  | Success |  9.395 μs | 0.1828 μs | 0.2176 μs |  0.99 |    0.03 | 0.4883 |   8.65 KB |        1.00 |
