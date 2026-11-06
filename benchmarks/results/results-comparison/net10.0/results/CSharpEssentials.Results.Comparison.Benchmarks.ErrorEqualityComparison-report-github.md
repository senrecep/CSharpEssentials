```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **6.371 ns** |   **0.1016 ns** |   **0.0950 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      6.350 ns |   0.1559 ns |   0.1458 ns |  1.00 |    0.03 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **28.973 ns** |   **0.2144 ns** |   **0.1790 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     30.756 ns |   0.1690 ns |   0.1581 ns |  1.06 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **32.282 ns** |   **0.4957 ns** |   **0.4637 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     75.549 ns |   0.7478 ns |   0.6995 ns |  2.34 |    0.04 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **82.964 ns** |   **0.4557 ns** |   **0.4040 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     66.331 ns |   0.4449 ns |   0.3715 ns |  0.80 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        | **10,342.030 ns** | **112.4725 ns** | **105.2069 ns** |  **1.00** |    **0.01** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  7,111.568 ns |  52.3152 ns |  48.9357 ns |  0.69 |    0.01 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **17,862.602 ns** | **127.5556 ns** | **106.5147 ns** |  **1.00** |    **0.01** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 13,643.837 ns | 109.3880 ns | 102.3216 ns |  0.76 |    0.01 | 0.3204 |    5392 B |        1.00 |
