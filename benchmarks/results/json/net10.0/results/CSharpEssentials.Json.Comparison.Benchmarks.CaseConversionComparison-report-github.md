```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **2,350.00 ns** | **16.285 ns** | **15.233 ns** |  **1.00** |    **0.01** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    36.82 ns |  0.637 ns |  0.596 ns |  0.02 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,483.56 ns |  3.523 ns |  3.123 ns |  1.48 |    0.01 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **557.50 ns** |  **0.363 ns** |  **0.340 ns** |  **1.00** |    **0.00** | **0.0086** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    16.42 ns |  0.108 ns |  0.101 ns |  0.03 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,015.03 ns |  2.574 ns |  2.408 ns |  1.82 |    0.00 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |   **106.88 ns** |  **0.206 ns** |  **0.172 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    11.95 ns |  0.114 ns |  0.107 ns |  0.11 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   397.74 ns |  1.676 ns |  1.400 ns |  3.72 |    0.01 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **2,408.47 ns** |  **2.489 ns** |  **2.207 ns** |  **1.00** |    **0.00** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   736.50 ns |  5.017 ns |  4.693 ns |  0.31 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,475.46 ns | 51.018 ns | 47.722 ns |  2.69 |    0.02 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **569.87 ns** |  **3.062 ns** |  **2.864 ns** |  **1.00** |    **0.01** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   168.30 ns |  1.420 ns |  1.328 ns |  0.30 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,615.79 ns | 12.339 ns | 11.542 ns |  2.84 |    0.02 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |   **109.92 ns** |  **0.253 ns** |  **0.237 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    43.18 ns |  0.107 ns |  0.094 ns |  0.39 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   481.01 ns |  0.524 ns |  0.437 ns |  4.38 |    0.01 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **2,406.81 ns** |  **2.409 ns** |  **2.253 ns** |  **1.00** |    **0.00** | **0.0381** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   734.42 ns |  1.632 ns |  1.526 ns |  0.31 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,249.49 ns |  5.332 ns |  4.988 ns |  2.60 |    0.00 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **558.58 ns** |  **0.918 ns** |  **0.814 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   165.51 ns |  0.407 ns |  0.361 ns |  0.30 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,539.08 ns |  1.474 ns |  1.231 ns |  2.76 |    0.00 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |   **110.23 ns** |  **0.305 ns** |  **0.270 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    43.11 ns |  0.101 ns |  0.095 ns |  0.39 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   464.84 ns |  1.137 ns |  0.949 ns |  4.22 |    0.01 | 0.0067 |     112 B |        2.00 |
