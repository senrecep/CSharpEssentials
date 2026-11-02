```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **0.3633 ns** | **0.0022 ns** | **0.0020 ns** | **0.3632 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 0.3632 ns | 0.0030 ns | 0.0025 ns | 0.3640 ns |  1.00 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **0.6790 ns** | **0.0129 ns** | **0.0114 ns** | **0.6781 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 0.6327 ns | 0.0067 ns | 0.0063 ns | 0.6332 ns |  0.93 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.2929 ns** | **0.0019 ns** | **0.0018 ns** | **0.2929 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.2889 ns | 0.0013 ns | 0.0011 ns | 0.2890 ns |  0.99 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.0006 ns** | **0.0007 ns** | **0.0007 ns** | **0.0005 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.0057 ns | 0.0025 ns | 0.0023 ns | 0.0055 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.3661 ns** | **0.0024 ns** | **0.0022 ns** | **0.3657 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.3634 ns | 0.0058 ns | 0.0054 ns | 0.3630 ns |  0.99 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **0.7335 ns** | **0.0115 ns** | **0.0102 ns** | **0.7317 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 0.6416 ns | 0.0093 ns | 0.0082 ns | 0.6410 ns |  0.87 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0677 ns** | **0.0035 ns** | **0.0031 ns** | **1.0664 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0479 ns | 0.0023 ns | 0.0022 ns | 1.0468 ns |  0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1387 ns** | **0.0271 ns** | **0.0253 ns** | **1.1250 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.1013 ns | 0.0093 ns | 0.0087 ns | 1.0987 ns |  0.97 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.5038 ns** | **0.0039 ns** | **0.0036 ns** | **0.5037 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.4112 ns | 0.0023 ns | 0.0019 ns | 0.4109 ns |  0.82 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **1.0077 ns** | **0.0063 ns** | **0.0059 ns** | **1.0087 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 0.9903 ns | 0.0065 ns | 0.0058 ns | 0.9910 ns |  0.98 |    0.01 |         - |          NA |
