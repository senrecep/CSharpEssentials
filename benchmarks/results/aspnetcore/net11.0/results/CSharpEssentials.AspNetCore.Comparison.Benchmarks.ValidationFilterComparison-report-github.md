```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2

Categories=Validate  

```
| Method                          | Rules | Outcome | Mean     | Error    | StdDev   | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------ |-------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **Bcl_Inline**                      | **1**     | **Invalid** | **17.24 μs** | **0.342 μs** | **0.714 μs** | **17.11 μs** |  **1.00** |    **0.06** | **0.7324** |  **12.83 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Invalid | 26.12 μs | 0.516 μs | 0.457 μs | 26.00 μs |  1.52 |    0.07 | 0.9766 |  16.53 KB |        1.29 |
| SharpGrip_AutoValidation        | 1     | Invalid | 33.95 μs | 0.662 μs | 0.680 μs | 33.90 μs |  1.97 |    0.09 | 0.9766 |     19 KB |        1.48 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **1**     | **Valid**   | **16.34 μs** | **0.353 μs** | **1.040 μs** | **16.08 μs** |  **1.00** |    **0.09** | **0.4883** |  **11.79 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 1     | Valid   | 18.06 μs | 0.358 μs | 0.755 μs | 18.01 μs |  1.11 |    0.08 | 0.7324 |  12.49 KB |        1.06 |
| SharpGrip_AutoValidation        | 1     | Valid   | 29.42 μs | 0.550 μs | 0.695 μs | 29.49 μs |  1.81 |    0.12 | 0.9766 |  17.14 KB |        1.45 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Invalid** | **19.25 μs** | **0.586 μs** | **1.633 μs** | **18.67 μs** |  **1.01** |    **0.11** | **0.7324** |  **12.97 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Invalid | 28.42 μs | 0.491 μs | 0.435 μs | 28.39 μs |  1.49 |    0.11 | 0.9766 |  17.63 KB |        1.36 |
| SharpGrip_AutoValidation        | 3     | Invalid | 41.23 μs | 0.647 μs | 0.573 μs | 41.11 μs |  2.16 |    0.17 | 1.4648 |  26.38 KB |        2.03 |
|                                 |       |         |          |          |          |          |       |         |        |           |             |
| **Bcl_Inline**                      | **3**     | **Valid**   | **16.50 μs** | **0.329 μs** | **0.966 μs** | **16.24 μs** |  **1.00** |    **0.08** | **0.4883** |  **11.79 KB** |        **1.00** |
| CSharpEssentials_WithValidation | 3     | Valid   | 17.39 μs | 0.335 μs | 0.480 μs | 17.51 μs |  1.06 |    0.07 | 0.7324 |  12.75 KB |        1.08 |
| SharpGrip_AutoValidation        | 3     | Valid   | 34.94 μs | 0.676 μs | 0.664 μs | 34.88 μs |  2.12 |    0.13 | 1.2207 |  21.05 KB |        1.79 |
