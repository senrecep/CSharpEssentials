```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **5.5239 ns** | **0.0014 ns** | **0.0012 ns** | **5.5239 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 5.5216 ns | 0.0023 ns | 0.0021 ns | 5.5213 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **6.8054 ns** | **0.0042 ns** | **0.0039 ns** | **6.8061 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 6.8984 ns | 0.0241 ns | 0.0214 ns | 6.9001 ns |  1.01 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0005 ns** | **0.0005 ns** | **0.0005 ns** | **0.0004 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0000 ns | 0.0001 ns | 0.0001 ns | 0.0000 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **4.8723 ns** | **0.0015 ns** | **0.0014 ns** | **4.8725 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 4.8725 ns | 0.0036 ns | 0.0028 ns | 4.8716 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.2915 ns** | **0.0021 ns** | **0.0018 ns** | **0.2909 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.2888 ns | 0.0005 ns | 0.0004 ns | 0.2888 ns |  0.99 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.2840 ns** | **0.0026 ns** | **0.0023 ns** | **0.2832 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.2919 ns | 0.0009 ns | 0.0008 ns | 0.2919 ns |  1.03 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.5725 ns** | **0.0020 ns** | **0.0018 ns** | **0.5731 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.5793 ns | 0.0029 ns | 0.0027 ns | 0.5789 ns |  1.01 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **6.8914 ns** | **0.0063 ns** | **0.0059 ns** | **6.8909 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 6.9427 ns | 0.0070 ns | 0.0066 ns | 6.9425 ns |  1.01 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0647 ns** | **0.0005 ns** | **0.0004 ns** | **1.0645 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0654 ns | 0.0019 ns | 0.0015 ns | 1.0648 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.0828 ns** | **0.0008 ns** | **0.0006 ns** | **1.0827 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.0830 ns | 0.0007 ns | 0.0006 ns | 1.0828 ns |  1.00 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.5699 ns** | **0.0015 ns** | **0.0014 ns** | **0.5700 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.5836 ns | 0.0053 ns | 0.0047 ns | 0.5820 ns |  1.02 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **0.9108 ns** | **0.0055 ns** | **0.0051 ns** | **0.9092 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 1.1013 ns | 0.0009 ns | 0.0008 ns | 1.1012 ns |  1.21 |    0.01 |         - |          NA |
