```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.219 ns** | **0.0016 ns** | **0.0013 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 124.880 ns | 0.2765 ns | 0.2587 ns |  23.93 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 374.696 ns | 0.6008 ns | 0.5619 ns |  71.79 |    0.11 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.628 ns** | **0.1094 ns** | **0.0913 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 138.443 ns | 0.3325 ns | 0.3110 ns |   8.33 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 366.754 ns | 3.3578 ns | 3.1409 ns |  22.06 |    0.22 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.238 ns** | **0.0019 ns** | **0.0016 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 213.856 ns | 2.1136 ns | 1.9771 ns |  40.83 |    0.37 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 580.445 ns | 0.8402 ns | 0.7448 ns | 110.82 |    0.14 | 0.0181 |     304 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.644 ns** | **0.0836 ns** | **0.0741 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 216.887 ns | 1.2721 ns | 1.1900 ns |  13.03 |    0.09 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 575.495 ns | 0.4423 ns | 0.4137 ns |  34.58 |    0.15 | 0.0210 |     360 B |        6.43 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.237 ns** | **0.0039 ns** | **0.0035 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 364.299 ns | 3.3562 ns | 3.1394 ns |  69.56 |    0.58 | 0.0243 |     408 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 735.091 ns | 1.6227 ns | 1.3551 ns | 140.36 |    0.26 | 0.0420 |     712 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.701 ns** | **0.0019 ns** | **0.0016 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 472.594 ns | 2.1744 ns | 2.0339 ns |  82.89 |    0.35 | 0.0553 |     928 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 842.448 ns | 2.2647 ns | 2.0076 ns | 147.77 |    0.34 | 0.0734 |    1232 B |          NA |
