```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **8.128 ns** |  **0.0654 ns** |  **0.0579 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      8.131 ns |  0.0088 ns |  0.0073 ns |  1.00 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **43.443 ns** |  **0.0168 ns** |  **0.0158 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     44.717 ns |  0.0294 ns |  0.0261 ns |  1.03 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **41.822 ns** |  **0.0691 ns** |  **0.0646 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     27.223 ns |  0.0805 ns |  0.0753 ns |  0.65 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **89.232 ns** |  **0.1894 ns** |  **0.1582 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     87.717 ns |  0.1321 ns |  0.1171 ns |  0.98 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,555.170 ns** | **24.6718 ns** | **20.6020 ns** |  **1.00** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  6,154.638 ns | 15.9222 ns | 14.8937 ns |  0.94 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **16,171.247 ns** |  **9.0252 ns** |  **7.5365 ns** |  **1.00** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 15,298.233 ns | 15.0358 ns | 14.0645 ns |  0.95 | 0.3052 |    5392 B |        1.00 |
