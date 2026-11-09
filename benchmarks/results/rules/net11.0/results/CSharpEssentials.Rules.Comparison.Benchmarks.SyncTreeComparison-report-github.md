```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method           | Shape   | Input    | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------- |-------- |--------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **HandWritten**      | **Linear5** | **Pass**     |   **2.845 ns** | **0.0047 ns** | **0.0041 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Linear5 | Pass     |  69.061 ns | 0.1158 ns | 0.1027 ns |  24.28 |    0.05 |      - |         - |          NA |
| CseEvaluateAsync | Linear5 | Pass     | 316.352 ns | 2.0069 ns | 1.8773 ns | 111.20 |    0.66 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Linear5** | **FailLast** |  **11.291 ns** | **0.0916 ns** | **0.0812 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | Linear5 | FailLast |  81.956 ns | 0.3464 ns | 0.3240 ns |   7.26 |    0.06 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | Linear5 | FailLast | 325.959 ns | 2.3158 ns | 2.1662 ns |  28.87 |    0.27 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **Pass**     |   **2.836 ns** | **0.0054 ns** | **0.0045 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | And5    | Pass     |  44.559 ns | 0.0734 ns | 0.0687 ns |  15.71 |    0.03 |      - |         - |          NA |
| CseEvaluateAsync | And5    | Pass     | 376.943 ns | 0.8399 ns | 0.7856 ns | 132.93 |    0.34 |      - |         - |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **And5**    | **FailLast** |  **11.361 ns** | **0.0607 ns** | **0.0568 ns** |   **1.00** |    **0.01** | **0.0033** |      **56 B** |        **1.00** |
| CseEvaluate      | And5    | FailLast |  58.944 ns | 0.1680 ns | 0.1571 ns |   5.19 |    0.03 | 0.0033 |      56 B |        1.00 |
| CseEvaluateAsync | And5    | FailLast | 357.473 ns | 0.7256 ns | 0.6788 ns |  31.47 |    0.16 | 0.0033 |      56 B |        1.00 |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **Pass**     |   **2.894 ns** | **0.0059 ns** | **0.0056 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | Pass     | 119.258 ns | 1.0959 ns | 1.0251 ns |  41.21 |    0.35 | 0.0134 |     224 B |          NA |
| CseEvaluateAsync | Or5     | Pass     | 442.495 ns | 2.1917 ns | 2.0502 ns | 152.91 |    0.74 | 0.0134 |     224 B |          NA |
|                  |         |          |            |           |           |        |         |        |           |             |
| **HandWritten**      | **Or5**     | **FailLast** |   **3.110 ns** | **0.0087 ns** | **0.0073 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CseEvaluate      | Or5     | FailLast | 190.202 ns | 1.0612 ns | 0.9927 ns |  61.16 |    0.34 | 0.0277 |     464 B |          NA |
| CseEvaluateAsync | Or5     | FailLast | 533.287 ns | 2.3711 ns | 2.2179 ns | 171.48 |    0.79 | 0.0277 |     464 B |          NA |
