```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                 | Categories | Length | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------- |------- |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Camel** | **Camel**      | **Long**   | **1,019.44 ns** |  **3.218 ns** |  **2.853 ns** |  **1.00** |    **0.00** | **0.0343** |     **584 B** |        **1.00** |
| Bcl_Camel              | Camel      | Long   |    35.97 ns |  0.726 ns |  0.606 ns |  0.04 |    0.00 | 0.0349 |     584 B |        1.00 |
| Humanizer_Camel        | Camel      | Long   | 3,603.89 ns |  6.073 ns |  5.681 ns |  3.54 |    0.01 | 0.0916 |    1576 B |        2.70 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Medium** |   **222.84 ns** |  **0.895 ns** |  **0.793 ns** |  **1.00** |    **0.00** | **0.0091** |     **152 B** |        **1.00** |
| Bcl_Camel              | Camel      | Medium |    19.43 ns |  0.379 ns |  0.355 ns |  0.09 |    0.00 | 0.0091 |     152 B |        1.00 |
| Humanizer_Camel        | Camel      | Medium | 1,064.87 ns |  2.125 ns |  1.987 ns |  4.78 |    0.02 | 0.0420 |     712 B |        4.68 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Camel** | **Camel**      | **Short**  |    **54.22 ns** |  **0.089 ns** |  **0.074 ns** |  **1.00** |    **0.00** | **0.0029** |      **48 B** |        **1.00** |
| Bcl_Camel              | Camel      | Short  |    13.71 ns |  0.239 ns |  0.223 ns |  0.25 |    0.00 | 0.0029 |      48 B |        1.00 |
| Humanizer_Camel        | Camel      | Short  |   437.39 ns |  1.685 ns |  1.576 ns |  8.07 |    0.03 | 0.0300 |     504 B |       10.50 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Long**   | **1,052.57 ns** |  **2.902 ns** |  **2.715 ns** |  **1.00** |    **0.00** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Long   |   733.99 ns |  1.489 ns |  1.243 ns |  0.70 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Long   | 6,887.76 ns | 21.757 ns | 20.351 ns |  6.54 |    0.02 | 0.1144 |    2016 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Medium** |   **262.35 ns** |  **0.704 ns** |  **0.588 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Medium |   184.83 ns |  0.646 ns |  0.604 ns |  0.70 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Medium | 1,717.00 ns |  2.413 ns |  2.139 ns |  6.54 |    0.02 | 0.0305 |     528 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Kebab** | **Kebab**      | **Short**  |    **60.73 ns** |  **0.158 ns** |  **0.123 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Kebab              | Kebab      | Short  |    58.97 ns |  0.201 ns |  0.178 ns |  0.97 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Kebab        | Kebab      | Short  |   518.65 ns |  1.716 ns |  1.605 ns |  8.54 |    0.03 | 0.0095 |     168 B |        3.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Long**   | **1,040.42 ns** |  **3.672 ns** |  **3.435 ns** |  **1.00** |    **0.00** | **0.0401** |     **672 B** |        **1.00** |
| Bcl_Snake              | Snake      | Long   |   744.68 ns |  2.117 ns |  1.877 ns |  0.72 |    0.00 | 0.0401 |     672 B |        1.00 |
| Humanizer_Snake        | Snake      | Long   | 6,816.86 ns | 36.035 ns | 33.707 ns |  6.55 |    0.04 | 0.0763 |    1344 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Medium** |   **267.35 ns** |  **0.795 ns** |  **0.744 ns** |  **1.00** |    **0.00** | **0.0105** |     **176 B** |        **1.00** |
| Bcl_Snake              | Snake      | Medium |   183.85 ns |  0.600 ns |  0.562 ns |  0.69 |    0.00 | 0.0105 |     176 B |        1.00 |
| Humanizer_Snake        | Snake      | Medium | 1,705.91 ns |  1.720 ns |  1.525 ns |  6.38 |    0.02 | 0.0210 |     352 B |        2.00 |
|                        |            |        |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Snake** | **Snake**      | **Short**  |    **61.74 ns** |  **0.233 ns** |  **0.218 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| Bcl_Snake              | Snake      | Short  |    58.52 ns |  0.143 ns |  0.127 ns |  0.95 |    0.00 | 0.0033 |      56 B |        1.00 |
| Humanizer_Snake        | Snake      | Short  |   518.29 ns |  1.803 ns |  1.686 ns |  8.40 |    0.04 | 0.0067 |     112 B |        2.00 |
