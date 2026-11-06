```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **2,484.74 ns** | **23.745 ns** | **22.211 ns** |  **1.00** |    **0.01** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    39.21 ns |  0.851 ns |  0.980 ns |  0.02 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,595.10 ns |  5.199 ns |  4.864 ns |  1.45 |    0.01 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **600.89 ns** |  **8.298 ns** |  **7.762 ns** |  **1.00** |    **0.02** | **0.0086** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    19.01 ns |  0.285 ns |  0.266 ns |  0.03 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,133.04 ns |  4.267 ns |  3.991 ns |  1.89 |    0.02 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |   **118.80 ns** |  **0.198 ns** |  **0.175 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    14.22 ns |  0.097 ns |  0.090 ns |  0.12 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   502.78 ns |  3.948 ns |  3.693 ns |  4.23 |    0.03 | 0.0296 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **2,468.21 ns** | **23.783 ns** | **22.247 ns** |  **1.00** |    **0.01** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   925.45 ns |  2.354 ns |  1.965 ns |  0.37 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 7,051.23 ns | 10.328 ns |  8.624 ns |  2.86 |    0.03 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **617.26 ns** |  **0.882 ns** |  **0.737 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   217.43 ns |  1.858 ns |  1.551 ns |  0.35 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,751.26 ns |  0.975 ns |  0.761 ns |  2.84 |    0.00 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |   **123.10 ns** |  **0.846 ns** |  **0.706 ns** |  **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    55.60 ns |  0.096 ns |  0.080 ns |  0.45 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   553.61 ns |  0.433 ns |  0.362 ns |  4.50 |    0.03 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **2,640.85 ns** | **51.649 ns** | **50.726 ns** |  **1.00** |    **0.03** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   927.29 ns |  3.570 ns |  2.788 ns |  0.35 |    0.01 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,860.30 ns | 39.920 ns | 37.341 ns |  2.60 |    0.05 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **635.57 ns** |  **0.512 ns** |  **0.399 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   216.74 ns |  1.507 ns |  1.336 ns |  0.34 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,780.52 ns | 10.377 ns |  9.707 ns |  2.80 |    0.01 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |   **131.27 ns** |  **0.418 ns** |  **0.391 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    55.30 ns |  0.159 ns |  0.141 ns |  0.42 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   541.08 ns |  0.871 ns |  0.727 ns |  4.12 |    0.01 | 0.0067 |     112 B |        2.00 |
