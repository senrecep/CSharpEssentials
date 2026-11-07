```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **1,051.73 ns** |  **2.845 ns** |  **2.522 ns** |  **1.00** |    **0.00** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    32.43 ns |  0.377 ns |  0.315 ns |  0.03 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,427.89 ns |  5.245 ns |  4.906 ns |  3.26 |    0.01 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **245.52 ns** |  **0.746 ns** |  **0.698 ns** |  **1.00** |    **0.00** | **0.0091** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    14.61 ns |  0.255 ns |  0.226 ns |  0.06 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium |   994.77 ns |  2.869 ns |  2.684 ns |  4.05 |    0.02 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |    **56.52 ns** |  **0.317 ns** |  **0.297 ns** |  **1.00** |    **0.01** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    11.00 ns |  0.059 ns |  0.055 ns |  0.19 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   400.03 ns |  2.263 ns |  2.117 ns |  7.08 |    0.05 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   |   **995.83 ns** |  **6.411 ns** |  **5.997 ns** |  **1.00** |    **0.01** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   823.34 ns |  2.369 ns |  2.216 ns |  0.83 |    0.01 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,011.88 ns | 12.233 ns | 11.443 ns |  6.04 |    0.04 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **249.95 ns** |  **0.726 ns** |  **0.679 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   190.02 ns |  0.383 ns |  0.339 ns |  0.76 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,522.28 ns |  1.477 ns |  1.309 ns |  6.09 |    0.02 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |    **58.08 ns** |  **0.138 ns** |  **0.115 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    45.86 ns |  0.162 ns |  0.151 ns |  0.79 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   472.71 ns |  2.244 ns |  2.099 ns |  8.14 |    0.04 | 0.0100 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   |   **991.35 ns** |  **4.202 ns** |  **3.931 ns** |  **1.00** |    **0.01** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   894.45 ns |  2.656 ns |  2.485 ns |  0.90 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,083.06 ns | 11.086 ns |  9.827 ns |  6.14 |    0.03 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **245.66 ns** |  **0.900 ns** |  **0.798 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   189.92 ns |  0.605 ns |  0.536 ns |  0.77 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,535.71 ns |  2.297 ns |  2.148 ns |  6.25 |    0.02 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |    **58.95 ns** |  **0.268 ns** |  **0.237 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    48.17 ns |  0.247 ns |  0.231 ns |  0.82 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   483.98 ns |  1.103 ns |  0.978 ns |  8.21 |    0.04 | 0.0067 |     112 B |        2.00 |
