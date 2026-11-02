```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **6.9249 ns** | **0.0033 ns** | **0.0030 ns** | **6.9243 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 5.5394 ns | 0.0102 ns | 0.0090 ns | 5.5391 ns |  0.80 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **6.8085 ns** | **0.0110 ns** | **0.0102 ns** | **6.8113 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 6.9186 ns | 0.0071 ns | 0.0067 ns | 6.9169 ns |  1.02 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0008 ns** | **0.0010 ns** | **0.0009 ns** | **0.0006 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0824 ns | 0.0398 ns | 0.1172 ns | 0.0007 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **4.8714 ns** | **0.0011 ns** | **0.0010 ns** | **4.8710 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 4.8705 ns | 0.0011 ns | 0.0010 ns | 4.8701 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.2910 ns** | **0.0010 ns** | **0.0009 ns** | **0.2907 ns** | **1.000** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.000 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.3027 ns** | **0.0007 ns** | **0.0006 ns** | **0.3027 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.2940 ns | 0.0021 ns | 0.0018 ns | 0.2939 ns |  0.97 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.5687 ns** | **0.0017 ns** | **0.0016 ns** | **0.5685 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.5802 ns | 0.0023 ns | 0.0021 ns | 0.5802 ns |  1.02 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **6.8967 ns** | **0.0107 ns** | **0.0100 ns** | **6.8915 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 6.9216 ns | 0.0061 ns | 0.0057 ns | 6.9224 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0643 ns** | **0.0005 ns** | **0.0004 ns** | **1.0643 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0647 ns | 0.0013 ns | 0.0010 ns | 1.0642 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1092 ns** | **0.0041 ns** | **0.0039 ns** | **1.1089 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.0832 ns | 0.0018 ns | 0.0016 ns | 1.0823 ns |  0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.8659 ns** | **0.0005 ns** | **0.0004 ns** | **0.8658 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.5841 ns | 0.0024 ns | 0.0022 ns | 0.5847 ns |  0.67 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **0.9165 ns** | **0.0029 ns** | **0.0027 ns** | **0.9163 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 1.1010 ns | 0.0007 ns | 0.0005 ns | 1.1009 ns |  1.20 |    0.00 |         - |          NA |
