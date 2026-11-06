```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.474 ns** | **0.0037 ns** | **0.0035 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 139.916 ns | 1.0025 ns | 0.9378 ns |  25.56 |    0.17 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 405.882 ns | 1.1740 ns | 1.0982 ns |  74.14 |    0.20 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **18.153 ns** | **0.1376 ns** | **0.1287 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 152.224 ns | 1.3300 ns | 1.2440 ns |   8.39 |    0.09 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 423.177 ns | 0.7683 ns | 0.7187 ns |  23.31 |    0.16 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.095 ns** | **0.0254 ns** | **0.0238 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 235.795 ns | 0.1297 ns | 0.1150 ns |  46.28 |    0.21 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 603.761 ns | 3.3482 ns | 3.1319 ns | 118.49 |    0.80 | 0.0181 |     312 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **17.996 ns** | **0.2736 ns** | **0.2560 ns** |   **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 230.833 ns | 0.7220 ns | 0.6400 ns |  12.83 |    0.18 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 632.876 ns | 1.0002 ns | 0.8867 ns |  35.17 |    0.49 | 0.0219 |     368 B |        6.57 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.398 ns** | **0.0076 ns** | **0.0067 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 404.980 ns | 0.9992 ns | 0.9346 ns |  75.02 |    0.19 | 0.0243 |     408 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 789.588 ns | 3.1082 ns | 2.7553 ns | 146.26 |    0.52 | 0.0429 |     720 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **6.064 ns** | **0.0034 ns** | **0.0028 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 510.809 ns | 3.2971 ns | 3.0842 ns |  84.24 |    0.49 | 0.0553 |     928 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 940.984 ns | 6.4354 ns | 6.0196 ns | 155.18 |    0.96 | 0.0725 |    1240 B |          NA |
