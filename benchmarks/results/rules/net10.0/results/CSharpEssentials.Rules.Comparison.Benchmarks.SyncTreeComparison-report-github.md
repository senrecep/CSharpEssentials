```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.244 ns** | **0.0093 ns** | **0.0082 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  94.004 ns | 0.1182 ns | 0.1106 ns |  17.93 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 365.362 ns | 0.3165 ns | 0.2643 ns |  69.67 |    0.12 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.949 ns** | **0.1160 ns** | **0.1028 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 107.903 ns | 0.2324 ns | 0.2061 ns |   6.37 |    0.04 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 304.708 ns | 0.1091 ns | 0.0911 ns |  17.98 |    0.11 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.188 ns** | **0.0094 ns** | **0.0079 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 269.142 ns | 0.2807 ns | 0.2625 ns |  51.88 |    0.09 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 479.170 ns | 0.9935 ns | 0.9294 ns |  92.36 |    0.22 | 0.0181 |     304 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.645 ns** | **0.0822 ns** | **0.0728 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 184.847 ns | 0.1247 ns | 0.1105 ns |  11.11 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 508.286 ns | 2.7029 ns | 2.5283 ns |  30.54 |    0.20 | 0.0210 |     360 B |        6.43 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.641 ns** | **0.0476 ns** | **0.0445 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 316.453 ns | 3.1498 ns | 2.9463 ns |  56.10 |    0.66 | 0.0243 |     408 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 624.226 ns | 1.7542 ns | 1.5550 ns | 110.66 |    0.88 | 0.0420 |     712 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.589 ns** | **0.0090 ns** | **0.0080 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 398.888 ns | 4.3121 ns | 4.0336 ns |  71.36 |    0.71 | 0.0553 |     928 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 721.615 ns | 3.1037 ns | 2.9032 ns | 129.10 |    0.53 | 0.0734 |    1232 B |          NA |
