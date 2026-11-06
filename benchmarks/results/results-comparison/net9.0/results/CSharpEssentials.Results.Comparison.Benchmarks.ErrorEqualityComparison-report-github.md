```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **8.805 ns** |   **0.1591 ns** |   **0.1488 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      8.517 ns |   0.0814 ns |   0.0722 ns |  0.97 |    0.02 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **46.801 ns** |   **0.7175 ns** |   **0.6712 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     47.635 ns |   0.2873 ns |   0.2688 ns |  1.02 |    0.02 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **33.577 ns** |   **0.2197 ns** |   **0.1948 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     28.173 ns |   0.2520 ns |   0.2234 ns |  0.84 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **86.103 ns** |   **0.6645 ns** |   **0.5890 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     90.013 ns |   1.7856 ns |   2.1257 ns |  1.05 |    0.03 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,951.115 ns** |  **55.3282 ns** |  **51.7541 ns** |  **1.00** |    **0.01** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  6,245.728 ns |  34.7720 ns |  32.5257 ns |  0.90 |    0.01 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **15,404.429 ns** | **112.1353 ns** | **104.8915 ns** |  **1.00** |    **0.01** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 15,781.778 ns |  83.9678 ns |  74.4353 ns |  1.02 |    0.01 | 0.3052 |    5392 B |        1.00 |
