```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **6.064 ns** |  **0.0310 ns** |  **0.0290 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      6.178 ns |  0.0330 ns |  0.0309 ns |  1.02 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **28.129 ns** |  **0.0476 ns** |  **0.0422 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     29.905 ns |  0.0366 ns |  0.0342 ns |  1.06 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **37.817 ns** |  **0.1065 ns** |  **0.0944 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     74.036 ns |  0.0310 ns |  0.0290 ns |  1.96 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **88.526 ns** |  **0.1848 ns** |  **0.1638 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     62.997 ns |  0.0391 ns |  0.0366 ns |  0.71 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        | **10,173.842 ns** |  **8.4954 ns** |  **7.5310 ns** |  **1.00** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  5,407.757 ns |  9.4193 ns |  7.8656 ns |  0.53 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **18,361.398 ns** | **16.3797 ns** | **14.5201 ns** |  **1.00** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 13,207.228 ns | 12.2285 ns | 10.8402 ns |  0.72 | 0.3204 |    5392 B |        1.00 |
