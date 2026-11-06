```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **5.5385 ns** | **0.0160 ns** | **0.0150 ns** | **5.5348 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 6.8108 ns | 0.0131 ns | 0.0102 ns | 6.8113 ns |  1.23 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **7.0648 ns** | **0.0386 ns** | **0.0361 ns** | **7.0642 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 7.1051 ns | 0.0674 ns | 0.0630 ns | 7.1142 ns |  1.01 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **4.8533 ns** | **0.0010 ns** | **0.0009 ns** | **4.8529 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 4.8788 ns | 0.0123 ns | 0.0096 ns | 4.8773 ns |  1.01 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.0325 ns** | **0.0102 ns** | **0.0095 ns** | **0.0334 ns** |  **1.09** |    **0.46** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.0018 ns | 0.0035 ns | 0.0033 ns | 0.0000 ns |  0.06 |    0.11 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.2998 ns** | **0.0025 ns** | **0.0021 ns** | **0.2995 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.3057 ns | 0.0146 ns | 0.0137 ns | 0.3120 ns |  1.02 |    0.04 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.6797 ns** | **0.0024 ns** | **0.0019 ns** | **0.6793 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.5743 ns | 0.0021 ns | 0.0018 ns | 0.5749 ns |  0.85 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **6.9490 ns** | **0.0070 ns** | **0.0065 ns** | **6.9502 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 7.0771 ns | 0.0029 ns | 0.0027 ns | 7.0773 ns |  1.02 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0665 ns** | **0.0014 ns** | **0.0011 ns** | **1.0668 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0684 ns | 0.0020 ns | 0.0018 ns | 1.0684 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1125 ns** | **0.0084 ns** | **0.0079 ns** | **1.1111 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.0967 ns | 0.0025 ns | 0.0021 ns | 1.0967 ns |  0.99 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.5971 ns** | **0.0047 ns** | **0.0042 ns** | **0.5970 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.5758 ns | 0.0020 ns | 0.0019 ns | 0.5762 ns |  0.96 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **1.0273 ns** | **0.0182 ns** | **0.0170 ns** | **1.0199 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 0.8730 ns | 0.0101 ns | 0.0095 ns | 0.8718 ns |  0.85 |    0.02 |         - |          NA |
