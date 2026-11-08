```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.488 ns** | **0.0102 ns** | **0.0095 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 116.535 ns | 0.1724 ns | 0.1529 ns |  21.24 |    0.04 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 400.122 ns | 0.6919 ns | 0.6472 ns |  72.91 |    0.17 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **18.139 ns** | **0.1210 ns** | **0.1132 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 128.454 ns | 0.5787 ns | 0.5130 ns |   7.08 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 413.906 ns | 0.7329 ns | 0.6855 ns |  22.82 |    0.14 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.036 ns** | **0.0109 ns** | **0.0096 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  71.092 ns | 0.0984 ns | 0.0872 ns |  14.12 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 447.177 ns | 1.3223 ns | 1.2369 ns |  88.80 |    0.29 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **18.725 ns** | **0.2242 ns** | **0.1872 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  81.714 ns | 0.2856 ns | 0.2385 ns |   4.36 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 484.083 ns | 1.7148 ns | 1.6040 ns |  25.85 |    0.26 | 0.0029 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.239 ns** | **0.0111 ns** | **0.0086 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 178.868 ns | 1.0070 ns | 0.9420 ns |  34.14 |    0.18 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 565.863 ns | 2.0304 ns | 1.6954 ns | 108.01 |    0.36 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **6.067 ns** | **0.0172 ns** | **0.0153 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 273.962 ns | 2.3278 ns | 2.1775 ns |  45.16 |    0.36 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 681.265 ns | 3.2271 ns | 3.0186 ns | 112.30 |    0.55 | 0.0277 |     464 B |          NA |
