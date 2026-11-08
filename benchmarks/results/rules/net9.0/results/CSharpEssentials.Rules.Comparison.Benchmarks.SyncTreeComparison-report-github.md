```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Median     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.234 ns** | **0.0083 ns** | **0.0073 ns** |   **5.233 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 123.864 ns | 0.3648 ns | 0.3413 ns | 124.004 ns |  23.67 |    0.07 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 342.164 ns | 0.4648 ns | 0.4121 ns | 342.093 ns |  65.37 |    0.12 |      - |         - |          NA |
|                  |         |          |            |           |           |            |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.955 ns** | **0.1897 ns** | **0.1774 ns** |  **16.858 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 135.913 ns | 0.2151 ns | 0.1907 ns | 135.863 ns |   8.02 |    0.08 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 372.402 ns | 0.7239 ns | 0.6771 ns | 372.296 ns |  21.97 |    0.22 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |            |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.225 ns** | **0.0067 ns** | **0.0063 ns** |   **5.225 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 207.135 ns | 0.1460 ns | 0.1365 ns | 207.086 ns |  39.64 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 424.277 ns | 8.4883 ns | 9.4347 ns | 432.444 ns |  81.20 |    1.76 |      - |         - |          NA |
|                  |         |          |            |           |           |            |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.185 ns** | **0.0931 ns** | **0.0825 ns** |  **16.175 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 206.183 ns | 2.1682 ns | 2.0281 ns | 206.970 ns |  12.74 |    0.14 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 421.560 ns | 1.4320 ns | 1.2694 ns | 421.292 ns |  26.05 |    0.15 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |            |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.227 ns** | **0.0051 ns** | **0.0047 ns** |   **5.225 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 315.050 ns | 1.1210 ns | 1.0486 ns | 314.806 ns |  60.27 |    0.20 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 512.687 ns | 2.4876 ns | 2.3269 ns | 512.472 ns |  98.08 |    0.44 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |            |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.442 ns** | **0.0029 ns** | **0.0026 ns** |   **5.441 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 417.271 ns | 4.0561 ns | 3.7941 ns | 416.994 ns |  76.68 |    0.68 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 637.097 ns | 4.0220 ns | 3.7622 ns | 637.854 ns | 117.07 |    0.67 | 0.0277 |     464 B |          NA |
