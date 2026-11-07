```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **2.836 ns** | **0.0022 ns** | **0.0020 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  90.465 ns | 0.0675 ns | 0.0632 ns |  31.90 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 291.406 ns | 0.2211 ns | 0.1846 ns | 102.77 |    0.09 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **11.120 ns** | **0.0401 ns** | **0.0355 ns** |   **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 103.886 ns | 0.4836 ns | 0.4287 ns |   9.34 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 308.990 ns | 0.5753 ns | 0.5382 ns |  27.79 |    0.10 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **2.833 ns** | **0.0016 ns** | **0.0013 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 161.404 ns | 0.1188 ns | 0.1053 ns |  56.97 |    0.04 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 360.340 ns | 1.5071 ns | 1.4098 ns | 127.19 |    0.49 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **11.070 ns** | **0.0952 ns** | **0.0891 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 189.178 ns | 0.2516 ns | 0.2231 ns |  17.09 |    0.13 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 446.577 ns | 0.4585 ns | 0.4289 ns |  40.35 |    0.32 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **2.887 ns** | **0.0014 ns** | **0.0012 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 275.786 ns | 0.4299 ns | 0.3590 ns |  95.52 |    0.13 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 445.085 ns | 1.3383 ns | 1.2518 ns | 154.15 |    0.42 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **3.016 ns** | **0.0013 ns** | **0.0012 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 332.180 ns | 2.5263 ns | 2.3631 ns | 110.14 |    0.76 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 511.256 ns | 2.6560 ns | 2.4844 ns | 169.52 |    0.80 | 0.0277 |     464 B |          NA |
