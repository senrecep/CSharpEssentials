```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **4.988 ns** | **0.0147 ns** | **0.0137 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  96.669 ns | 0.3038 ns | 0.2537 ns |  19.38 |    0.07 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 301.415 ns | 0.4690 ns | 0.4158 ns |  60.43 |    0.18 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.465 ns** | **0.1089 ns** | **0.1019 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast |  85.785 ns | 0.2620 ns | 0.2451 ns |   5.21 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 314.941 ns | 0.7976 ns | 0.7461 ns |  19.13 |    0.12 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **4.983 ns** | **0.0088 ns** | **0.0078 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  48.326 ns | 0.0856 ns | 0.0759 ns |   9.70 |    0.02 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 361.209 ns | 0.5525 ns | 0.4898 ns |  72.49 |    0.15 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.205 ns** | **0.0884 ns** | **0.0827 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  62.522 ns | 0.3387 ns | 0.3168 ns |   3.86 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 382.029 ns | 1.1499 ns | 1.0756 ns |  23.58 |    0.13 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.255 ns** | **0.0148 ns** | **0.0131 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 130.956 ns | 0.5636 ns | 0.4996 ns |  24.92 |    0.11 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 476.722 ns | 1.5619 ns | 1.3846 ns |  90.72 |    0.34 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.524 ns** | **0.0081 ns** | **0.0076 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 217.825 ns | 2.1354 ns | 1.9975 ns |  39.43 |    0.35 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 570.477 ns | 3.8306 ns | 3.5832 ns | 103.27 |    0.64 | 0.0277 |     464 B |          NA |
