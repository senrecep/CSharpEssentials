```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2

Categories=Validate  

```
| Method                          | Rules | Outcome | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_Inline**                      | **1**     | **Invalid** | **16.99 μs** | **0.338 μs** | **0.644 μs** |  **1.00** |    **0.05** | **0.4883** |  **11.14 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Invalid | 31.60 μs | 0.632 μs | 0.591 μs |  1.86 |    0.08 | 0.7324 |  14.22 KB |        1.28 |
| SharpGrip_AutoValidation        | 1     | Invalid | 40.51 μs | 0.515 μs | 0.457 μs |  2.39 |    0.09 | 0.9766 |  17.38 KB |        1.56 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **1**     | **Valid**   | **14.18 μs** | **0.275 μs** | **0.394 μs** |  **1.00** |    **0.04** | **0.6104** |  **10.18 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Valid   | 16.81 μs | 0.336 μs | 0.737 μs |  1.19 |    0.06 | 0.4883 |  11.13 KB |        1.09 |
| SharpGrip_AutoValidation        | 1     | Valid   | 32.76 μs | 0.630 μs | 0.700 μs |  2.31 |    0.08 | 0.7324 |  15.52 KB |        1.52 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Invalid** | **16.83 μs** | **0.328 μs** | **0.575 μs** |  **1.00** |    **0.05** | **0.4883** |   **11.2 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Invalid | 34.09 μs | 0.670 μs | 0.871 μs |  2.03 |    0.09 | 0.7324 |  15.36 KB |        1.37 |
| SharpGrip_AutoValidation        | 3     | Invalid | 50.83 μs | 0.494 μs | 0.462 μs |  3.02 |    0.11 | 1.4648 |  24.88 KB |        2.22 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Valid**   | **14.12 μs** | **0.229 μs** | **0.225 μs** |  **1.00** |    **0.02** | **0.6104** |  **10.18 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Valid   | 18.14 μs | 0.360 μs | 0.727 μs |  1.28 |    0.05 | 0.4883 |  11.41 KB |        1.12 |
| SharpGrip_AutoValidation        | 3     | Valid   | 38.05 μs | 0.309 μs | 0.289 μs |  2.69 |    0.05 | 0.9766 |  19.49 KB |        1.91 |
