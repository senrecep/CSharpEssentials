```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.254 ns** | **0.0149 ns** | **0.0139 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  99.403 ns | 0.1056 ns | 0.0987 ns |  18.92 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 343.834 ns | 0.8213 ns | 0.6858 ns |  65.44 |    0.21 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.523 ns** | **0.1550 ns** | **0.1450 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 110.011 ns | 0.3891 ns | 0.3640 ns |   6.66 |    0.06 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 374.558 ns | 1.2132 ns | 1.0755 ns |  22.67 |    0.20 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.234 ns** | **0.0073 ns** | **0.0064 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  62.187 ns | 0.1040 ns | 0.0973 ns |  11.88 |    0.02 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 428.930 ns | 3.0412 ns | 2.5396 ns |  81.96 |    0.48 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.551 ns** | **0.1141 ns** | **0.1067 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  65.733 ns | 0.2844 ns | 0.2660 ns |   3.97 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 453.741 ns | 0.7223 ns | 0.6756 ns |  27.42 |    0.18 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.244 ns** | **0.0082 ns** | **0.0068 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 150.943 ns | 2.0232 ns | 1.7935 ns |  28.78 |    0.33 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 530.858 ns | 2.6418 ns | 2.4712 ns | 101.23 |    0.47 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.451 ns** | **0.0078 ns** | **0.0069 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 233.726 ns | 2.4809 ns | 2.1992 ns |  42.88 |    0.39 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 637.719 ns | 2.8324 ns | 2.6494 ns | 116.99 |    0.49 | 0.0277 |     464 B |          NA |
