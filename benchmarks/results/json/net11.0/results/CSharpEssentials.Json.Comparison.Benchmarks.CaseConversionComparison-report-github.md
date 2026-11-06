```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **2,447.31 ns** |  **2.865 ns** |  **2.680 ns** |  **1.00** |    **0.00** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    35.44 ns |  0.690 ns |  0.612 ns |  0.01 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,507.09 ns |  4.661 ns |  4.360 ns |  1.43 |    0.00 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **565.12 ns** |  **0.381 ns** |  **0.318 ns** |  **1.00** |    **0.00** | **0.0086** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    16.20 ns |  0.218 ns |  0.204 ns |  0.03 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,014.30 ns |  2.329 ns |  2.178 ns |  1.79 |    0.00 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |   **111.71 ns** |  **0.203 ns** |  **0.190 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    11.28 ns |  0.104 ns |  0.093 ns |  0.10 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   398.58 ns |  5.113 ns |  4.783 ns |  3.57 |    0.04 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **2,503.77 ns** |  **7.767 ns** |  **6.486 ns** |  **1.00** |    **0.00** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   833.69 ns |  1.478 ns |  1.383 ns |  0.33 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,090.09 ns |  7.114 ns |  6.306 ns |  2.43 |    0.01 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **592.65 ns** |  **0.783 ns** |  **0.654 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   196.97 ns |  0.381 ns |  0.356 ns |  0.33 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,564.54 ns |  2.120 ns |  1.983 ns |  2.64 |    0.00 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |   **113.40 ns** |  **0.280 ns** |  **0.262 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    47.22 ns |  0.197 ns |  0.184 ns |  0.42 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   492.32 ns |  0.618 ns |  0.548 ns |  4.34 |    0.01 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **2,500.28 ns** |  **2.147 ns** |  **1.904 ns** |  **1.00** |    **0.00** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   836.97 ns |  1.205 ns |  1.068 ns |  0.33 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,057.94 ns | 13.551 ns | 12.675 ns |  2.42 |    0.01 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **592.39 ns** |  **0.590 ns** |  **0.552 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   193.58 ns |  0.270 ns |  0.240 ns |  0.33 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,537.28 ns |  3.907 ns |  3.655 ns |  2.60 |    0.01 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |   **113.12 ns** |  **0.141 ns** |  **0.125 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    47.90 ns |  0.127 ns |  0.119 ns |  0.42 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   457.72 ns |  1.824 ns |  1.706 ns |  4.05 |    0.02 | 0.0067 |     112 B |        2.00 |
