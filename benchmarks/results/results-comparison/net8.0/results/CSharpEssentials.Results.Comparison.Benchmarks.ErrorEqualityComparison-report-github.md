```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **8.416 ns** |  **0.0056 ns** |  **0.0049 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      8.450 ns |  0.0386 ns |  0.0362 ns |  1.00 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **45.436 ns** |  **0.0105 ns** |  **0.0088 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     45.810 ns |  0.3601 ns |  0.3368 ns |  1.01 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **41.338 ns** |  **0.0624 ns** |  **0.0554 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     27.051 ns |  0.0974 ns |  0.0911 ns |  0.65 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **94.933 ns** |  **0.2524 ns** |  **0.2237 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     87.090 ns |  0.1112 ns |  0.0986 ns |  0.92 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,473.350 ns** | **26.4289 ns** | **24.7216 ns** |  **1.00** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  5,762.013 ns | 23.7894 ns | 21.0887 ns |  0.89 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **16,611.856 ns** | **14.0865 ns** | **11.7628 ns** |  **1.00** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 15,607.398 ns | 21.6987 ns | 20.2970 ns |  0.94 | 0.3052 |    5392 B |        1.00 |
