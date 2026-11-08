```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                       | Categories  | WithMetadata | Mean          | Error       | StdDev      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------- |------------ |------------- |--------------:|------------:|------------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Equals**      | **Equals**      | **False**        |      **9.102 ns** |   **0.1218 ns** |   **0.1139 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | False        |      9.118 ns |   0.0671 ns |   0.0627 ns |  1.00 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_Equals**      | **Equals**      | **True**         |     **48.681 ns** |   **0.4216 ns** |   **0.3943 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_Equals               | Equals      | True         |     49.831 ns |   1.0066 ns |   0.9886 ns |  1.02 |    0.02 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **False**        |     **32.818 ns** |   **0.4577 ns** |   **0.4282 ns** |  **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | False        |     28.300 ns |   0.1866 ns |   0.1654 ns |  0.86 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_GetHashCode** | **GetHashCode** | **True**         |     **87.748 ns** |   **0.9397 ns** |   **0.8790 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| ErrorOr_GetHashCode          | GetHashCode | True         |     90.589 ns |   0.5623 ns |   0.4984 ns |  1.03 |    0.01 |      - |         - |          NA |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **False**        |  **6,604.562 ns** |  **60.8826 ns** |  **56.9497 ns** |  **1.00** |    **0.01** | **0.3204** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | False        |  6,246.707 ns |  56.7552 ns |  53.0888 ns |  0.95 |    0.01 | 0.3204 |    5392 B |        1.00 |
|                              |             |              |               |             |             |       |         |        |           |             |
| **CSharpEssentials_HashSet**     | **HashSet**     | **True**         | **16,515.790 ns** | **147.3371 ns** | **130.6105 ns** |  **1.00** |    **0.01** | **0.3052** |    **5392 B** |        **1.00** |
| ErrorOr_HashSet              | HashSet     | True         | 16,577.879 ns | 207.8986 ns | 194.4685 ns |  1.00 |    0.01 | 0.3052 |    5392 B |        1.00 |
