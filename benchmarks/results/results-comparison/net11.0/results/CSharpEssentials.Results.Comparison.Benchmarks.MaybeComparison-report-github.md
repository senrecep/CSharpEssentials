```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **0.3808 ns** | **0.0200 ns** | **0.0187 ns** | **0.3774 ns** |  **1.00** |    **0.07** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 0.3796 ns | 0.0145 ns | 0.0128 ns | 0.3836 ns |  1.00 |    0.06 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **0.6898 ns** | **0.0147 ns** | **0.0138 ns** | **0.6913 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 0.6360 ns | 0.0041 ns | 0.0100 ns | 0.6344 ns |  0.92 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.2922 ns** | **0.0014 ns** | **0.0013 ns** | **0.2919 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.2957 ns | 0.0076 ns | 0.0071 ns | 0.2938 ns |  1.01 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.0022 ns** | **0.0055 ns** | **0.0046 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.0025 ns | 0.0008 ns | 0.0007 ns | 0.0023 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.3709 ns** | **0.0080 ns** | **0.0071 ns** | **0.3699 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.3662 ns | 0.0069 ns | 0.0065 ns | 0.3648 ns |  0.99 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **0.6781 ns** | **0.0072 ns** | **0.0067 ns** | **0.6755 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 0.6341 ns | 0.0083 ns | 0.0069 ns | 0.6344 ns |  0.94 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0679 ns** | **0.0039 ns** | **0.0035 ns** | **1.0668 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0475 ns | 0.0014 ns | 0.0013 ns | 1.0469 ns |  0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1266 ns** | **0.0167 ns** | **0.0156 ns** | **1.1249 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.0982 ns | 0.0079 ns | 0.0070 ns | 1.0964 ns |  0.98 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.4966 ns** | **0.0095 ns** | **0.0079 ns** | **0.4982 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.4105 ns | 0.0046 ns | 0.0041 ns | 0.4101 ns |  0.83 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **0.8724 ns** | **0.0105 ns** | **0.0093 ns** | **0.8681 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 0.9877 ns | 0.0445 ns | 0.0416 ns | 0.9737 ns |  1.13 |    0.05 |         - |          NA |
