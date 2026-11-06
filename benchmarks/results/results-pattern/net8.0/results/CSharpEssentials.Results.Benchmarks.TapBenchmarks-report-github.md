```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **20.28 ns** |  **0.151 ns** |  **0.141 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    22.42 ns |  0.197 ns |  0.175 ns |  1.11 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    30.66 ns |  0.140 ns |  0.124 ns |  1.51 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    44.48 ns |  0.097 ns |  0.091 ns |  2.19 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    44.32 ns |  0.062 ns |  0.052 ns |  2.19 |    0.01 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    24.71 ns |  0.151 ns |  0.142 ns |  1.22 |    0.01 | 0.0081 |     136 B |        1.70 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **878.84 ns** |  **6.294 ns** |  **5.887 ns** |  **1.00** |    **0.01** | **0.0172** |     **311 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   856.09 ns |  6.424 ns |  6.009 ns |  0.97 |    0.01 | 0.0172 |     288 B |        0.93 |
| TaskSource_TaskHandler             | False   | True  |   887.65 ns |  5.818 ns |  5.442 ns |  1.01 |    0.01 | 0.0219 |     376 B |        1.21 |
| ValueTaskSource_Action             | False   | True  |   916.46 ns | 12.432 ns | 12.210 ns |  1.04 |    0.02 | 0.0219 |     367 B |        1.18 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   908.17 ns |  4.382 ns |  4.099 ns |  1.03 |    0.01 | 0.0238 |     399 B |        1.28 |
| Instance_TaskHandler               | False   | True  |    24.92 ns |  0.315 ns |  0.295 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.44 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **24.99 ns** |  **0.328 ns** |  **0.307 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    23.77 ns |  0.270 ns |  0.253 ns |  0.95 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    42.44 ns |  0.257 ns |  0.240 ns |  1.70 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    30.80 ns |  0.033 ns |  0.027 ns |  1.23 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    34.85 ns |  0.025 ns |  0.023 ns |  1.40 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    20.84 ns |  0.147 ns |  0.137 ns |  0.83 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,431.16 ns** |  **8.121 ns** |  **7.597 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   869.87 ns |  5.243 ns |  4.648 ns |  0.61 |    0.00 | 0.0134 |     232 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,650.28 ns |  9.426 ns |  8.817 ns |  1.15 |    0.01 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_Action             | True    | True  |   881.35 ns |  6.332 ns |  5.923 ns |  0.62 |    0.01 | 0.0181 |     311 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,590.55 ns | 14.864 ns | 13.904 ns |  1.11 |    0.01 | 0.0229 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   856.47 ns |  5.747 ns |  5.376 ns |  0.60 |    0.00 | 0.0143 |     239 B |        0.68 |
