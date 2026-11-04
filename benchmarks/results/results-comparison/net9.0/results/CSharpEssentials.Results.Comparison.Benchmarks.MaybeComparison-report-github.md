```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **5.5669 ns** | **0.0191 ns** | **0.0159 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 5.5318 ns | 0.0069 ns | 0.0065 ns |  0.99 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **7.0559 ns** | **0.0301 ns** | **0.0281 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 7.1115 ns | 0.0073 ns | 0.0068 ns |  1.01 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **4.8555 ns** | **0.0030 ns** | **0.0028 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 4.8543 ns | 0.0016 ns | 0.0015 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.0247 ns** | **0.0034 ns** | **0.0030 ns** |  **1.01** |    **0.17** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.0007 ns | 0.0008 ns | 0.0006 ns |  0.03 |    0.03 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.2997 ns** | **0.0019 ns** | **0.0018 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.2925 ns | 0.0017 ns | 0.0015 ns |  0.98 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.5652 ns** | **0.0027 ns** | **0.0025 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.5753 ns | 0.0035 ns | 0.0033 ns |  1.02 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **6.9346 ns** | **0.0032 ns** | **0.0029 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 7.0943 ns | 0.0071 ns | 0.0059 ns |  1.02 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0689 ns** | **0.0037 ns** | **0.0034 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0482 ns | 0.0010 ns | 0.0009 ns |  0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1089 ns** | **0.0050 ns** | **0.0042 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.0969 ns | 0.0048 ns | 0.0045 ns |  0.99 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.5879 ns** | **0.0026 ns** | **0.0023 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.6792 ns | 0.0026 ns | 0.0025 ns |  1.16 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **1.0377 ns** | **0.0150 ns** | **0.0140 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 0.8727 ns | 0.0054 ns | 0.0051 ns |  0.84 |    0.01 |         - |          NA |
