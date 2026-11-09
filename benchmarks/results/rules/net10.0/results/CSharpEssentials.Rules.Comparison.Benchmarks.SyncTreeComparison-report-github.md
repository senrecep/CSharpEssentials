```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **4.977 ns** | **0.0058 ns** | **0.0051 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  71.667 ns | 0.1528 ns | 0.1355 ns |  14.40 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 315.547 ns | 1.1368 ns | 1.0078 ns |  63.40 |    0.21 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.233 ns** | **0.1174 ns** | **0.1098 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast |  92.459 ns | 0.3912 ns | 0.3659 ns |   5.70 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 320.147 ns | 5.1226 ns | 5.4811 ns |  19.72 |    0.35 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **4.975 ns** | **0.0052 ns** | **0.0043 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  47.302 ns | 0.1560 ns | 0.1302 ns |   9.51 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 372.233 ns | 0.9321 ns | 0.8719 ns |  74.82 |    0.18 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.211 ns** | **0.1005 ns** | **0.0891 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  60.573 ns | 0.2178 ns | 0.1930 ns |   3.74 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 389.920 ns | 0.5430 ns | 0.4534 ns |  24.05 |    0.13 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.272 ns** | **0.0048 ns** | **0.0043 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 138.188 ns | 2.3062 ns | 2.1573 ns |  26.21 |    0.40 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 479.787 ns | 2.2383 ns | 2.0937 ns |  91.00 |    0.39 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.614 ns** | **0.0115 ns** | **0.0107 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 219.347 ns | 1.7862 ns | 1.6708 ns |  39.07 |    0.30 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 587.084 ns | 2.0925 ns | 1.9573 ns | 104.57 |    0.39 | 0.0277 |     464 B |          NA |
