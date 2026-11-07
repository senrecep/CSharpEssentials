```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **4.969 ns** | **0.0045 ns** | **0.0040 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     | 118.212 ns | 0.0529 ns | 0.0469 ns |  23.79 |    0.02 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 300.393 ns | 0.2533 ns | 0.2370 ns |  60.45 |    0.07 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **16.137 ns** | **0.1176 ns** | **0.1100 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast | 110.016 ns | 0.2487 ns | 0.2077 ns |   6.82 |    0.05 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 310.876 ns | 1.0725 ns | 0.8956 ns |  19.27 |    0.14 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **4.973 ns** | **0.0048 ns** | **0.0045 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     | 197.860 ns | 0.3472 ns | 0.3248 ns |  39.79 |    0.07 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 427.386 ns | 8.1944 ns | 8.4151 ns |  85.95 |    1.65 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **16.311 ns** | **0.2334 ns** | **0.2183 ns** |   **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast | 213.821 ns | 1.2251 ns | 1.1460 ns |  13.11 |    0.18 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 382.740 ns | 0.4773 ns | 0.4231 ns |  23.47 |    0.30 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **5.229 ns** | **0.0029 ns** | **0.0027 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 299.234 ns | 5.0714 ns | 4.7438 ns |  57.22 |    0.88 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 477.221 ns | 2.2541 ns | 2.1085 ns |  91.26 |    0.39 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **5.583 ns** | **0.0035 ns** | **0.0032 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 394.984 ns | 2.2810 ns | 2.1337 ns |  70.75 |    0.37 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 571.484 ns | 2.9832 ns | 2.7905 ns | 102.36 |    0.49 | 0.0277 |     464 B |          NA |
