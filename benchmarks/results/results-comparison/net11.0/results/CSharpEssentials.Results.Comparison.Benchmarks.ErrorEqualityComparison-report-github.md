```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **6.385 ns** |   **0.1614 ns** |   **0.2416 ns** |  **1.00** |    **0.05** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      6.245 ns |   0.1445 ns |   0.1664 ns |  0.98 |    0.04 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **30.304 ns** |   **0.3485 ns** |   **0.3260 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     33.199 ns |   0.0962 ns |   0.0803 ns |  1.10 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **31.101 ns** |   **0.1914 ns** |   **0.1697 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     27.044 ns |   0.0928 ns |   0.0775 ns |  0.87 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **69.908 ns** |   **1.0241 ns** |   **0.9580 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     68.913 ns |   0.4959 ns |   0.4396 ns |  0.99 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,598.451 ns** |  **56.1129 ns** |  **43.8093 ns** |  **1.00** |    **0.01** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  5,540.137 ns |  90.6571 ns |  84.8007 ns |  0.84 |    0.01 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **14,243.500 ns** | **112.1213 ns** |  **99.3926 ns** |  **1.00** |    **0.01** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 13,452.364 ns | 178.0944 ns | 166.5896 ns |  0.94 |    0.01 | 0.3204 |    5392 B |        1.00 |
