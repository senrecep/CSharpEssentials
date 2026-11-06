```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                  | Categories      | Percent | Mean            | Error        | StdDev       | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------ |---------------- |-------- |----------------:|-------------:|-------------:|-------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Unique** | **Unique**          | **1**       |    **10,037.72 ns** |    **37.356 ns** |    **29.165 ns** |   **1.00** |    **0.00** |      **-** |      **-** |      **64 B** |        **1.00** |
| Bcl_Unique              | Unique          | 1       | 1,299,519.62 ns | 1,318.271 ns | 1,168.613 ns | 129.46 |    0.38 |      - |      - |    4089 B |       63.89 |
| BclShared_Unique        | Unique          | 1       |     4,641.41 ns |    12.910 ns |    12.076 ns |   0.46 |    0.00 | 0.2441 |      - |    4088 B |       63.88 |
| HandWritten_Unique      | Unique          | 1       |       292.82 ns |     5.754 ns |     5.382 ns |   0.03 |    0.00 | 0.2441 | 0.0033 |    4088 B |       63.88 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **CSharpEssentials_Unique** | **Unique**          | **50**      |   **653,059.95 ns** | **1,315.712 ns** | **1,230.717 ns** |  **1.000** |    **0.00** |      **-** |      **-** |    **2024 B** |        **1.00** |
| Bcl_Unique              | Unique          | 50      | 1,302,465.55 ns | 2,669.116 ns | 2,496.693 ns |  1.994 |    0.01 |      - |      - |    6049 B |        2.99 |
| BclShared_Unique        | Unique          | 50      |     5,005.39 ns |    17.218 ns |    16.106 ns |  0.008 |    0.00 | 0.3586 |      - |    6048 B |        2.99 |
| HandWritten_Unique      | Unique          | 50      |     3,393.62 ns |    14.203 ns |    13.285 ns |  0.005 |    0.00 | 0.3586 | 0.0038 |    6048 B |        2.99 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **CSharpEssentials_Unique** | **Unique**          | **99**      | **1,302,933.68 ns** | **1,632.304 ns** | **1,526.858 ns** |  **1.000** |    **0.00** |      **-** |      **-** |    **3984 B** |        **1.00** |
| Bcl_Unique              | Unique          | 99      | 1,288,284.27 ns | 1,559.681 ns | 1,302.404 ns |  0.989 |    0.00 |      - |      - |    8009 B |        2.01 |
| BclShared_Unique        | Unique          | 99      |     4,909.41 ns |     9.161 ns |     8.569 ns |  0.004 |    0.00 | 0.4730 |      - |    8008 B |        2.01 |
| HandWritten_Unique      | Unique          | 99      |     6,645.63 ns |    11.870 ns |     9.912 ns |  0.005 |    0.00 | 0.4730 |      - |    8008 B |        2.01 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **1**       |        **68.65 ns** |     **0.148 ns** |     **0.139 ns** |      **?** |       **?** | **0.0038** |      **-** |      **64 B** |           **?** |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **50**      |     **3,164.13 ns** |     **3.919 ns** |     **3.666 ns** |      **?** |       **?** | **0.1183** |      **-** |    **2024 B** |           **?** |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **99**      |     **6,132.76 ns** |    **12.120 ns** |    **11.337 ns** |      **?** |       **?** | **0.2365** |      **-** |    **3984 B** |           **?** |
