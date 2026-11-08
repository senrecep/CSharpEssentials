```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.471 ns** | **0.0034 ns** | **0.0029 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 145.052 ns | 0.5257 ns | 0.4660 ns |  26.51 |    0.08 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 381.092 ns | 0.1366 ns | 0.1141 ns |  69.66 |    0.04 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **19.160 ns** | **0.0782 ns** | **0.0693 ns** |   **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 150.586 ns | 0.3567 ns | 0.3336 ns |   7.86 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 407.059 ns | 0.4903 ns | 0.4586 ns |  21.25 |    0.08 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.002 ns** | **0.0017 ns** | **0.0015 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 226.700 ns | 0.0999 ns | 0.0834 ns |  45.32 |    0.02 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 465.844 ns | 1.7939 ns | 1.6780 ns |  93.13 |    0.33 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **19.541 ns** | **0.1054 ns** | **0.0934 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 263.369 ns | 0.4264 ns | 0.3989 ns |  13.48 |    0.07 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 477.577 ns | 2.6793 ns | 2.5062 ns |  24.44 |    0.17 | 0.0029 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.225 ns** | **0.0083 ns** | **0.0078 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 341.918 ns | 1.1260 ns | 1.0532 ns |  65.44 |    0.22 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 575.956 ns | 1.6134 ns | 1.5091 ns | 110.23 |    0.32 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **6.096 ns** | **0.0030 ns** | **0.0028 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 461.251 ns | 2.3882 ns | 2.1171 ns |  75.66 |    0.34 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 679.960 ns | 4.8732 ns | 4.5584 ns | 111.54 |    0.73 | 0.0277 |     464 B |          NA |
