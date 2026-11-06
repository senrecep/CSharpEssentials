```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error      | StdDev     | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|-----------:|-----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **6.057 ns** |  **0.0095 ns** |  **0.0079 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      5.935 ns |  0.0467 ns |  0.0437 ns |  0.98 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **27.564 ns** |  **0.0398 ns** |  **0.0353 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     29.885 ns |  0.0846 ns |  0.0706 ns |  1.08 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **32.201 ns** |  **0.0262 ns** |  **0.0245 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     26.499 ns |  0.0187 ns |  0.0175 ns |  0.82 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **88.275 ns** |  **0.0448 ns** |  **0.0374 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     64.548 ns |  0.1080 ns |  0.0902 ns |  0.73 |      - |         - |          NA |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,455.556 ns** | **26.3886 ns** | **24.6839 ns** |  **1.00** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  5,217.701 ns | 10.3049 ns |  9.1350 ns |  0.81 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |            |            |       |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **14,871.272 ns** | **14.6791 ns** | **13.7308 ns** |  **1.00** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 13,217.930 ns | 24.0479 ns | 21.3178 ns |  0.89 | 0.3204 |    5392 B |        1.00 |
