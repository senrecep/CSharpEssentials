```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **2.845 ns** | **0.0227 ns** | **0.0213 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 100.534 ns | 0.5456 ns | 0.4836 ns |  35.34 |    0.30 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 314.386 ns | 2.2524 ns | 1.8809 ns | 110.53 |    1.02 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **11.982 ns** | **0.0737 ns** | **0.0690 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 101.484 ns | 0.1533 ns | 0.1359 ns |   8.47 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 330.696 ns | 3.8932 ns | 3.6417 ns |  27.60 |    0.33 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **2.850 ns** | **0.0188 ns** | **0.0167 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 201.855 ns | 1.2603 ns | 1.1788 ns |  70.83 |    0.57 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 467.886 ns | 1.1482 ns | 1.0178 ns | 164.18 |    0.99 | 0.0181 |     304 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **11.912 ns** | **0.0953 ns** | **0.0891 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 196.711 ns | 0.2233 ns | 0.1865 ns |  16.51 |    0.12 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 476.290 ns | 3.6645 ns | 3.4277 ns |  39.99 |    0.40 | 0.0210 |     360 B |        6.43 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **2.885 ns** | **0.0018 ns** | **0.0016 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 287.202 ns | 3.6586 ns | 3.4222 ns |  99.55 |    1.15 | 0.0243 |     408 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 593.441 ns | 5.6389 ns | 5.2746 ns | 205.71 |    1.77 | 0.0420 |     712 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **3.126 ns** | **0.0032 ns** | **0.0027 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 377.785 ns | 0.9887 ns | 0.8765 ns | 120.86 |    0.29 | 0.0553 |     928 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 654.554 ns | 2.3306 ns | 2.0660 ns | 209.40 |    0.66 | 0.0734 |    1232 B |          NA |
