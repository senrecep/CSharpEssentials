```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2

Categories=Validate  

```
| Method                          | Rules | Outcome | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_Inline**                      | **1**     | **Invalid** | **15.22 μs** | **0.303 μs** | **0.523 μs** | **15.14 μs** |  **1.00** |    **0.05** | **0.4883** |  **11.03 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Invalid | 28.26 μs | 0.523 μs | 0.622 μs | 28.36 μs |  1.86 |    0.07 | 0.7324 |  13.81 KB |        1.25 |
| SharpGrip_AutoValidation        | 1     | Invalid | 36.86 μs | 0.518 μs | 1.281 μs | 36.51 μs |  2.42 |    0.12 | 0.9766 |  17.25 KB |        1.56 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **1**     | **Valid**   | **13.27 μs** | **0.259 μs** | **0.327 μs** | **13.36 μs** |  **1.00** |    **0.03** | **0.6104** |  **10.07 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Valid   | 14.68 μs | 0.264 μs | 0.394 μs | 14.74 μs |  1.11 |    0.04 | 0.4883 |  10.78 KB |        1.07 |
| SharpGrip_AutoValidation        | 1     | Valid   | 27.88 μs | 0.555 μs | 0.958 μs | 27.89 μs |  2.10 |    0.09 | 0.7324 |  15.41 KB |        1.53 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Invalid** | **15.74 μs** | **0.308 μs** | **0.539 μs** | **15.64 μs** |  **1.00** |    **0.05** | **0.4883** |  **11.09 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Invalid | 30.97 μs | 0.480 μs | 0.472 μs | 30.89 μs |  1.97 |    0.07 | 0.7324 |  14.94 KB |        1.35 |
| SharpGrip_AutoValidation        | 3     | Invalid | 47.14 μs | 0.822 μs | 0.729 μs | 47.24 μs |  3.00 |    0.11 | 1.4648 |  24.75 KB |        2.23 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Valid**   | **13.42 μs** | **0.268 μs** | **0.668 μs** | **13.17 μs** |  **1.00** |    **0.07** | **0.6104** |  **10.07 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Valid   | 14.88 μs | 0.280 μs | 0.323 μs | 14.81 μs |  1.11 |    0.06 | 0.4883 |  11.05 KB |        1.10 |
| SharpGrip_AutoValidation        | 3     | Valid   | 36.38 μs | 0.594 μs | 0.556 μs | 36.31 μs |  2.72 |    0.13 | 0.9766 |  19.38 KB |        1.92 |
