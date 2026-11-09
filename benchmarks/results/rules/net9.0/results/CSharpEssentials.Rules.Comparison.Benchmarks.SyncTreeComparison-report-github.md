```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.254 ns** | **0.0091 ns** | **0.0085 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 100.557 ns | 0.1506 ns | 0.1409 ns |  19.14 |    0.04 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 371.538 ns | 2.7184 ns | 2.5428 ns |  70.71 |    0.48 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.683 ns** | **0.1142 ns** | **0.1068 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 110.133 ns | 0.3453 ns | 0.3230 ns |   6.60 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 384.542 ns | 0.9374 ns | 0.8310 ns |  23.05 |    0.15 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.230 ns** | **0.0102 ns** | **0.0096 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  53.358 ns | 0.0944 ns | 0.0883 ns |  10.20 |    0.02 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 429.482 ns | 0.4039 ns | 0.3778 ns |  82.13 |    0.16 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.536 ns** | **0.1110 ns** | **0.1038 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  71.919 ns | 0.3422 ns | 0.3200 ns |   4.35 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 446.346 ns | 0.5449 ns | 0.4254 ns |  26.99 |    0.17 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.275 ns** | **0.0098 ns** | **0.0086 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 150.585 ns | 1.3789 ns | 1.2224 ns |  28.55 |    0.23 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 520.468 ns | 3.0233 ns | 2.6801 ns |  98.67 |    0.52 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.455 ns** | **0.0090 ns** | **0.0084 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 241.583 ns | 1.7171 ns | 1.4339 ns |  44.28 |    0.26 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 648.539 ns | 3.5859 ns | 3.3542 ns | 118.88 |    0.62 | 0.0277 |     464 B |          NA |
