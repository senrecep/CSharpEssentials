```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   |   **893.53 ns** |  **2.353 ns** |  **2.201 ns** |  **1.00** |    **0.00** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    34.55 ns |  0.314 ns |  0.279 ns |  0.04 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,605.42 ns |  8.072 ns |  7.156 ns |  4.04 |    0.01 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **216.55 ns** |  **0.475 ns** |  **0.421 ns** |  **1.00** |    **0.00** | **0.0091** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    18.02 ns |  0.262 ns |  0.233 ns |  0.08 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,095.70 ns |  2.807 ns |  2.344 ns |  5.06 |    0.01 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |    **51.96 ns** |  **0.137 ns** |  **0.122 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    13.95 ns |  0.186 ns |  0.174 ns |  0.27 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   447.87 ns |  1.390 ns |  1.300 ns |  8.62 |    0.03 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **1,015.86 ns** |  **1.885 ns** |  **1.471 ns** |  **1.00** |    **0.00** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   919.25 ns |  2.842 ns |  2.519 ns |  0.90 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,902.96 ns | 30.960 ns | 27.445 ns |  6.80 |    0.03 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **251.49 ns** |  **0.866 ns** |  **0.810 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   214.36 ns |  0.496 ns |  0.387 ns |  0.85 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,790.83 ns |  5.312 ns |  4.969 ns |  7.12 |    0.03 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |    **58.65 ns** |  **0.243 ns** |  **0.227 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    54.90 ns |  0.165 ns |  0.146 ns |  0.94 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   562.44 ns |  2.668 ns |  2.496 ns |  9.59 |    0.05 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **1,014.75 ns** |  **4.930 ns** |  **4.612 ns** |  **1.00** |    **0.01** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   929.06 ns | 16.061 ns | 15.023 ns |  0.92 |    0.01 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,892.06 ns | 26.156 ns | 23.187 ns |  6.79 |    0.04 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **241.94 ns** |  **2.342 ns** |  **2.191 ns** |  **1.00** |    **0.01** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   219.96 ns |  4.322 ns |  4.043 ns |  0.91 |    0.02 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,767.53 ns |  3.915 ns |  3.662 ns |  7.31 |    0.07 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |    **60.09 ns** |  **0.290 ns** |  **0.271 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    54.70 ns |  0.300 ns |  0.280 ns |  0.91 |    0.01 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   553.68 ns |  1.463 ns |  1.368 ns |  9.21 |    0.05 | 0.0067 |     112 B |        2.00 |
