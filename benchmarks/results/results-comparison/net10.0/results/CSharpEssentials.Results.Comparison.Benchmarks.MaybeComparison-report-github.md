```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **0.3665 ns** | **0.0034 ns** | **0.0030 ns** | **0.3658 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 0.3642 ns | 0.0059 ns | 0.0055 ns | 0.3620 ns |  0.99 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **0.6781 ns** | **0.0041 ns** | **0.0036 ns** | **0.6783 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 0.6284 ns | 0.0026 ns | 0.0023 ns | 0.6288 ns |  0.93 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0001 ns** | **0.0001 ns** | **0.0001 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0006 ns | 0.0005 ns | 0.0005 ns | 0.0005 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **0.0003 ns** | **0.0004 ns** | **0.0004 ns** | **0.0000 ns** |     **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 0.4084 ns | 0.0425 ns | 0.1254 ns | 0.3411 ns |     ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.0122 ns** | **0.0011 ns** | **0.0009 ns** | **0.0120 ns** |  **1.00** |    **0.09** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.0020 ns | 0.0046 ns | 0.0039 ns | 0.0002 ns |  0.17 |    0.31 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.3026 ns** | **0.0013 ns** | **0.0012 ns** | **0.3026 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.2896 ns | 0.0004 ns | 0.0003 ns | 0.2897 ns |  0.96 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.3868 ns** | **0.0076 ns** | **0.0068 ns** | **0.3845 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.3576 ns | 0.0049 ns | 0.0041 ns | 0.3557 ns |  0.92 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **0.7175 ns** | **0.0129 ns** | **0.0121 ns** | **0.7184 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 0.6285 ns | 0.0036 ns | 0.0030 ns | 0.6285 ns |  0.88 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0639 ns** | **0.0033 ns** | **0.0029 ns** | **1.0633 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0473 ns | 0.0011 ns | 0.0010 ns | 1.0470 ns |  0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1824 ns** | **0.0084 ns** | **0.0078 ns** | **1.1795 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.1100 ns | 0.0090 ns | 0.0080 ns | 1.1079 ns |  0.94 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.4920 ns** | **0.0084 ns** | **0.0078 ns** | **0.4911 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.4346 ns | 0.0057 ns | 0.0053 ns | 0.4320 ns |  0.88 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |       |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **1.0175 ns** | **0.0064 ns** | **0.0057 ns** | **1.0161 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 1.0015 ns | 0.0271 ns | 0.0254 ns | 0.9912 ns |  0.98 |    0.02 |         - |          NA |
