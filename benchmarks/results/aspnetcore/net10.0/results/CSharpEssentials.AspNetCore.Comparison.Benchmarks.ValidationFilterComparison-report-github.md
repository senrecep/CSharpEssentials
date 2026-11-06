```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2

Categories=Validate  

```
| Method                          | Rules | Outcome | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_Inline**                      | **1**     | **Invalid** | **16.44 μs** | **0.327 μs** | **0.546 μs** |  **1.00** |    **0.05** | **0.4883** |   **11.5 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Invalid | 28.76 μs | 0.541 μs | 0.623 μs |  1.75 |    0.07 | 0.7324 |  15.17 KB |        1.32 |
| SharpGrip_AutoValidation        | 1     | Invalid | 34.11 μs | 0.502 μs | 0.445 μs |  2.08 |    0.07 | 0.9766 |  17.62 KB |        1.53 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **1**     | **Valid**   | **14.38 μs** | **0.278 μs** | **0.407 μs** |  **1.00** |    **0.04** | **0.6104** |  **10.46 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Valid   | 15.80 μs | 0.279 μs | 0.451 μs |  1.10 |    0.04 | 0.4883 |  11.17 KB |        1.07 |
| SharpGrip_AutoValidation        | 1     | Valid   | 29.39 μs | 0.327 μs | 0.255 μs |  2.05 |    0.06 | 0.7324 |  15.75 KB |        1.51 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Invalid** | **17.03 μs** | **0.337 μs** | **0.658 μs** |  **1.00** |    **0.05** | **0.4883** |  **11.64 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Invalid | 30.39 μs | 0.544 μs | 0.688 μs |  1.79 |    0.08 | 0.9766 |     16 KB |        1.37 |
| SharpGrip_AutoValidation        | 3     | Invalid | 43.86 μs | 0.566 μs | 0.502 μs |  2.58 |    0.10 | 1.4648 |     25 KB |        2.15 |
|                                 |       |         |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Valid**   | **14.27 μs** | **0.241 μs** | **0.225 μs** |  **1.00** |    **0.02** | **0.6104** |  **10.46 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Valid   | 16.33 μs | 0.319 μs | 0.458 μs |  1.14 |    0.04 | 0.4883 |  11.25 KB |        1.08 |
| SharpGrip_AutoValidation        | 3     | Valid   | 34.14 μs | 0.594 μs | 0.526 μs |  2.39 |    0.05 | 0.9766 |  19.65 KB |        1.88 |
