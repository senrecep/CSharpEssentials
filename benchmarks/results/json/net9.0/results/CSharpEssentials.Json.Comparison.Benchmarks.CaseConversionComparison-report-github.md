```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **2,425.49 ns** |  **6.876 ns** |  **6.096 ns** |  **1.00** |    **0.00** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    38.43 ns |  0.833 ns |  0.891 ns |  0.02 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,588.17 ns |  2.221 ns |  1.854 ns |  1.48 |    0.00 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **564.75 ns** |  **1.283 ns** |  **1.200 ns** |  **1.00** |    **0.00** | **0.0086** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    19.55 ns |  0.212 ns |  0.198 ns |  0.03 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,101.40 ns |  1.714 ns |  1.603 ns |  1.95 |    0.00 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |   **121.44 ns** |  **0.351 ns** |  **0.311 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    14.15 ns |  0.064 ns |  0.057 ns |  0.12 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   455.77 ns |  1.523 ns |  1.425 ns |  3.75 |    0.01 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **2,567.24 ns** |  **6.613 ns** |  **5.522 ns** |  **1.00** |    **0.00** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   756.91 ns |  0.926 ns |  0.773 ns |  0.29 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,715.63 ns | 16.122 ns | 14.292 ns |  2.62 |    0.01 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **580.94 ns** |  **0.599 ns** |  **0.531 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   184.15 ns |  0.388 ns |  0.324 ns |  0.32 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,750.77 ns |  9.192 ns |  8.598 ns |  3.01 |    0.01 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |   **122.89 ns** |  **0.102 ns** |  **0.085 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    58.86 ns |  0.166 ns |  0.147 ns |  0.48 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   535.44 ns |  0.384 ns |  0.321 ns |  4.36 |    0.00 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **2,517.16 ns** | **12.469 ns** | **11.054 ns** |  **1.00** |    **0.01** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   743.06 ns |  2.914 ns |  2.583 ns |  0.30 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,615.43 ns | 31.187 ns | 29.173 ns |  2.63 |    0.02 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **581.66 ns** |  **3.118 ns** |  **2.604 ns** |  **1.00** |    **0.01** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   184.92 ns |  0.372 ns |  0.330 ns |  0.32 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,644.08 ns |  0.696 ns |  0.582 ns |  2.83 |    0.01 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |   **121.29 ns** |  **0.229 ns** |  **0.203 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    58.74 ns |  0.167 ns |  0.156 ns |  0.48 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   508.16 ns |  4.946 ns |  4.626 ns |  4.19 |    0.04 | 0.0067 |     112 B |        2.00 |
