```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **5.490 ns** | **0.0102 ns** | **0.0085 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 116.216 ns | 0.1778 ns | 0.1485 ns |  21.17 |    0.04 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 400.230 ns | 0.8870 ns | 0.8297 ns |  72.90 |    0.18 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **18.704 ns** | **0.2487 ns** | **0.2077 ns** |   **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 128.914 ns | 0.5309 ns | 0.4966 ns |   6.89 |    0.08 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 408.115 ns | 0.7998 ns | 0.7482 ns |  21.82 |    0.23 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **5.026 ns** | **0.0107 ns** | **0.0100 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  70.036 ns | 0.0670 ns | 0.0627 ns |  13.93 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 495.862 ns | 2.8980 ns | 2.7108 ns |  98.66 |    0.56 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **19.220 ns** | **0.0789 ns** | **0.0738 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  81.752 ns | 0.2586 ns | 0.2419 ns |   4.25 |    0.02 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 465.346 ns | 0.7153 ns | 0.6691 ns |  24.21 |    0.10 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.029 ns** | **0.0087 ns** | **0.0077 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 174.603 ns | 0.5865 ns | 0.5486 ns |  34.72 |    0.12 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 599.190 ns | 1.3889 ns | 1.2313 ns | 119.14 |    0.29 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **6.041 ns** | **0.0178 ns** | **0.0166 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 268.180 ns | 1.2391 ns | 1.0984 ns |  44.40 |    0.21 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 676.207 ns | 2.2169 ns | 2.0737 ns | 111.94 |    0.45 | 0.0277 |     464 B |          NA |
