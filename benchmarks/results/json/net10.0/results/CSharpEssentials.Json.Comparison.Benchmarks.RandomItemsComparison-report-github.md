```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                  | Categories      | Percent | Mean            | Error        | StdDev       | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------ |---------------- |-------- |----------------:|-------------:|-------------:|-------:|--------:|-------:|-------:|----------:|------------:|
| **CSharpEssentials_Unique** | **Unique**          | **1**       |     **9,759.15 ns** |    **58.672 ns** |    **54.882 ns** |   **1.00** |    **0.01** |      **-** |      **-** |      **64 B** |        **1.00** |
| Bcl_Unique              | Unique          | 1       | 1,308,046.47 ns | 7,245.655 ns | 6,777.590 ns | 134.04 |    0.99 |      - |      - |    4089 B |       63.89 |
| BclShared_Unique        | Unique          | 1       |     5,260.72 ns |    11.709 ns |    10.380 ns |   0.54 |    0.00 | 0.2441 |      - |    4088 B |       63.88 |
| HandWritten_Unique      | Unique          | 1       |       317.61 ns |     6.259 ns |     6.147 ns |   0.03 |    0.00 | 0.2441 | 0.0033 |    4088 B |       63.88 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **CSharpEssentials_Unique** | **Unique**          | **50**      |   **657,812.08 ns** | **1,704.188 ns** | **1,510.718 ns** |  **1.000** |    **0.00** |      **-** |      **-** |    **2025 B** |        **1.00** |
| Bcl_Unique              | Unique          | 50      | 1,286,621.72 ns | 2,105.793 ns | 1,969.760 ns |  1.956 |    0.01 |      - |      - |    6048 B |        2.99 |
| BclShared_Unique        | Unique          | 50      |     5,407.57 ns |    12.057 ns |    10.688 ns |  0.008 |    0.00 | 0.3586 |      - |    6048 B |        2.99 |
| HandWritten_Unique      | Unique          | 50      |     3,500.12 ns |     7.296 ns |     6.825 ns |  0.005 |    0.00 | 0.3586 | 0.0038 |    6048 B |        2.99 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **CSharpEssentials_Unique** | **Unique**          | **99**      | **1,303,631.05 ns** | **2,145.125 ns** | **2,006.552 ns** |  **1.000** |    **0.00** |      **-** |      **-** |    **3984 B** |        **1.00** |
| Bcl_Unique              | Unique          | 99      | 1,282,952.20 ns | 1,270.235 ns | 1,188.179 ns |  0.984 |    0.00 |      - |      - |    8008 B |        2.01 |
| BclShared_Unique        | Unique          | 99      |     5,325.28 ns |    10.968 ns |     9.723 ns |  0.004 |    0.00 | 0.4730 |      - |    8008 B |        2.01 |
| HandWritten_Unique      | Unique          | 99      |     6,573.75 ns |     8.955 ns |     6.991 ns |  0.005 |    0.00 | 0.4730 |      - |    8008 B |        2.01 |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **1**       |        **67.63 ns** |     **0.153 ns** |     **0.127 ns** |      **?** |       **?** | **0.0038** |      **-** |      **64 B** |           **?** |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **50**      |     **3,080.06 ns** |     **5.108 ns** |     **4.778 ns** |      **?** |       **?** | **0.1183** |      **-** |    **2024 B** |           **?** |
|                         |                 |         |                 |              |              |        |         |        |        |           |             |
| **Bcl_WithReplacement**     | **WithReplacement** | **99**      |     **6,037.28 ns** |     **6.066 ns** |     **5.674 ns** |      **?** |       **?** | **0.2365** |      **-** |    **3984 B** |           **?** |
