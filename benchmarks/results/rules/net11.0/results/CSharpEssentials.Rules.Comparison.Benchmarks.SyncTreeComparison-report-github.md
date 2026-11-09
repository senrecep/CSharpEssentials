```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **2.841 ns** | **0.0066 ns** | **0.0056 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  71.536 ns | 0.0717 ns | 0.0599 ns |  25.18 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 347.234 ns | 6.7450 ns | 8.7704 ns | 122.24 |    3.04 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **11.536 ns** | **0.0813 ns** | **0.0761 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast |  85.199 ns | 0.3391 ns | 0.3172 ns |   7.39 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 304.892 ns | 0.6543 ns | 0.5109 ns |  26.43 |    0.18 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **2.840 ns** | **0.0095 ns** | **0.0088 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  44.488 ns | 0.0825 ns | 0.0644 ns |  15.67 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 353.412 ns | 0.3988 ns | 0.3730 ns | 124.46 |    0.40 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **11.327 ns** | **0.0924 ns** | **0.0864 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  55.735 ns | 0.2015 ns | 0.1786 ns |   4.92 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 364.515 ns | 1.3171 ns | 1.2320 ns |  32.18 |    0.26 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **2.780 ns** | **0.0051 ns** | **0.0043 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 112.621 ns | 0.5129 ns | 0.4546 ns |  40.51 |    0.17 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 447.701 ns | 1.9201 ns | 1.7960 ns | 161.03 |    0.67 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **3.058 ns** | **0.0064 ns** | **0.0060 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 185.004 ns | 0.9802 ns | 0.9169 ns |  60.50 |    0.31 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 552.079 ns | 2.1662 ns | 2.0263 ns | 180.55 |    0.73 | 0.0277 |     464 B |          NA |
