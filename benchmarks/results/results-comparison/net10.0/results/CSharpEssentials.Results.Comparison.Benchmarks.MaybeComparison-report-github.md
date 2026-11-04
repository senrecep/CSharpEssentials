```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                                       | Categories        | HasValue | Mean      | Error     | StdDev    | Median    | Ratio  | RatioSD | Allocated | Alloc Ratio |
|--------------------------------------------- |------------------ |--------- |----------:|----------:|----------:|----------:|-------:|--------:|----------:|------------:|
| **CSharpEssentials_Bind**                        | **Bind**              | **False**    | **0.3708 ns** | **0.0049 ns** | **0.0045 ns** | **0.3715 ns** |   **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | False    | 0.3658 ns | 0.0041 ns | 0.0039 ns | 0.3651 ns |   0.99 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Bind**                        | **Bind**              | **True**     | **0.6874 ns** | **0.0170 ns** | **0.0159 ns** | **0.6878 ns** |   **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Bind              | Bind              | True     | 0.6348 ns | 0.0046 ns | 0.0041 ns | 0.6345 ns |   0.92 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **False**    | **0.0003 ns** | **0.0006 ns** | **0.0005 ns** | **0.0000 ns** |      **?** |       **?** |         **-** |           **?** |
| CSharpFunctionalExtensions_Create            | Create            | False    | 0.0002 ns | 0.0005 ns | 0.0004 ns | 0.0000 ns |      ? |       ? |         - |           ? |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Create**                      | **Create**            | **True**     | **0.0015 ns** | **0.0008 ns** | **0.0007 ns** | **0.0014 ns** |   **1.25** |    **0.91** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Create            | Create            | True     | 0.2893 ns | 0.0007 ns | 0.0007 ns | 0.2890 ns | 243.33 |  128.80 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **False**    | **0.0114 ns** | **0.0019 ns** | **0.0017 ns** | **0.0110 ns** |   **1.02** |    **0.21** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | False    | 0.0006 ns | 0.0009 ns | 0.0008 ns | 0.0002 ns |   0.06 |    0.07 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_GetValueOrDefault**           | **GetValueOrDefault** | **True**     | **0.3009 ns** | **0.0016 ns** | **0.0015 ns** | **0.3007 ns** |   **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_GetValueOrDefault | GetValueOrDefault | True     | 0.2910 ns | 0.0009 ns | 0.0008 ns | 0.2906 ns |   0.97 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **False**    | **0.3652 ns** | **0.0018 ns** | **0.0016 ns** | **0.3651 ns** |   **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | False    | 0.3645 ns | 0.0032 ns | 0.0030 ns | 0.3643 ns |   1.00 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Map**                         | **Map**               | **True**     | **0.6830 ns** | **0.0147 ns** | **0.0137 ns** | **0.6765 ns** |   **1.00** |    **0.03** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Map               | Map               | True     | 0.6445 ns | 0.0107 ns | 0.0100 ns | 0.6413 ns |   0.94 |    0.02 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **False**    | **1.0651 ns** | **0.0009 ns** | **0.0007 ns** | **1.0652 ns** |   **1.00** |    **0.00** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | False    | 1.0465 ns | 0.0006 ns | 0.0005 ns | 1.0463 ns |   0.98 |    0.00 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Match**                       | **Match**             | **True**     | **1.1237 ns** | **0.0076 ns** | **0.0071 ns** | **1.1244 ns** |   **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Match             | Match             | True     | 1.1113 ns | 0.0117 ns | 0.0109 ns | 1.1090 ns |   0.99 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **False**    | **0.5462 ns** | **0.0046 ns** | **0.0041 ns** | **0.5452 ns** |   **1.00** |    **0.01** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | False    | 0.4119 ns | 0.0040 ns | 0.0036 ns | 0.4112 ns |   0.75 |    0.01 |         - |          NA |
|                                              |                   |          |           |           |           |           |        |         |           |             |
| **CSharpEssentials_Where**                       | **Where**             | **True**     | **1.0289 ns** | **0.0140 ns** | **0.0131 ns** | **1.0261 ns** |   **1.00** |    **0.02** |         **-** |          **NA** |
| CSharpFunctionalExtensions_Where             | Where             | True     | 1.0161 ns | 0.0176 ns | 0.0165 ns | 1.0165 ns |   0.99 |    0.02 |         - |          NA |
