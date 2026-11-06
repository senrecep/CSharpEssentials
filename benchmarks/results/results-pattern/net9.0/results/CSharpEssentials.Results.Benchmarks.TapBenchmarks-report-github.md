```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **21.80 ns** | **0.156 ns** | **0.146 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    20.59 ns | 0.090 ns | 0.080 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    31.82 ns | 0.205 ns | 0.192 ns |  1.46 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    51.50 ns | 0.120 ns | 0.112 ns |  2.36 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    51.85 ns | 0.136 ns | 0.121 ns |  2.38 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    25.10 ns | 0.172 ns | 0.160 ns |  1.15 |    0.01 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    20.14 ns | 0.080 ns | 0.075 ns |  0.92 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **860.52 ns** | **3.610 ns** | **3.377 ns** |  **1.00** |    **0.01** | **0.0181** |     **312 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   863.00 ns | 4.687 ns | 4.155 ns |  1.00 |    0.01 | 0.0172 |     287 B |        0.92 |
| TaskSource_TaskHandler             | False   | True  |   875.85 ns | 3.841 ns | 3.593 ns |  1.02 |    0.01 | 0.0219 |     375 B |        1.20 |
| ValueTaskSource_Action             | False   | True  |   880.33 ns | 9.251 ns | 7.223 ns |  1.02 |    0.01 | 0.0219 |     367 B |        1.18 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   884.38 ns | 6.839 ns | 6.397 ns |  1.03 |    0.01 | 0.0238 |     398 B |        1.28 |
| Instance_TaskHandler               | False   | True  |    25.93 ns | 0.487 ns | 0.455 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.44 |
| Instance_ValueTaskHandler          | False   | True  |    19.84 ns | 0.123 ns | 0.116 ns |  0.02 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **21.88 ns** | **0.180 ns** | **0.169 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    20.43 ns | 0.135 ns | 0.126 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    40.86 ns | 0.160 ns | 0.142 ns |  1.87 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    37.40 ns | 0.180 ns | 0.151 ns |  1.71 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    39.76 ns | 0.023 ns | 0.021 ns |  1.82 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    20.42 ns | 0.149 ns | 0.139 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    33.17 ns | 0.023 ns | 0.021 ns |  1.52 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,386.21 ns** | **9.769 ns** | **9.138 ns** |  **1.00** |    **0.01** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   852.60 ns | 5.172 ns | 4.838 ns |  0.62 |    0.01 | 0.0134 |     231 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,603.13 ns | 9.617 ns | 8.996 ns |  1.16 |    0.01 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_Action             | True    | True  |   873.36 ns | 3.702 ns | 3.463 ns |  0.63 |    0.00 | 0.0181 |     310 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,520.49 ns | 8.903 ns | 7.892 ns |  1.10 |    0.01 | 0.0248 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   840.48 ns | 4.397 ns | 4.113 ns |  0.61 |    0.00 | 0.0134 |     239 B |        0.68 |
| Instance_ValueTaskHandler          | True    | True  |   869.81 ns | 3.604 ns | 3.195 ns |  0.63 |    0.00 | 0.0143 |     254 B |        0.72 |
