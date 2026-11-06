```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **16.58 ns** |  **0.143 ns** |  **0.134 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    16.11 ns |  0.135 ns |  0.120 ns |  0.97 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    23.21 ns |  0.276 ns |  0.258 ns |  1.40 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    28.13 ns |  0.305 ns |  0.286 ns |  1.70 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    29.09 ns |  0.215 ns |  0.201 ns |  1.75 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    21.12 ns |  0.307 ns |  0.272 ns |  1.27 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    13.96 ns |  0.114 ns |  0.101 ns |  0.84 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **3,260.99 ns** | **30.943 ns** | **28.944 ns** | **1.000** |    **0.01** | **0.0153** |     **312 B** |        **1.00** |
| TaskSource_Action                  | False   | True  | 3,197.19 ns | 24.023 ns | 22.471 ns | 0.981 |    0.01 | 0.0153 |     288 B |        0.92 |
| TaskSource_TaskHandler             | False   | True  | 3,242.15 ns | 25.417 ns | 21.224 ns | 0.994 |    0.01 | 0.0191 |     376 B |        1.21 |
| ValueTaskSource_Action             | False   | True  | 3,187.17 ns | 26.751 ns | 23.714 ns | 0.977 |    0.01 | 0.0191 |     366 B |        1.17 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 3,180.08 ns | 43.440 ns | 40.634 ns | 0.975 |    0.01 | 0.0229 |     398 B |        1.28 |
| Instance_TaskHandler               | False   | True  |    21.78 ns |  0.463 ns |  0.664 ns | 0.007 |    0.00 | 0.0081 |     136 B |        0.44 |
| Instance_ValueTaskHandler          | False   | True  |    13.96 ns |  0.114 ns |  0.107 ns | 0.004 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **18.63 ns** |  **0.110 ns** |  **0.103 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    16.12 ns |  0.210 ns |  0.164 ns |  0.87 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    35.76 ns |  0.481 ns |  0.450 ns |  1.92 |    0.03 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    15.85 ns |  0.032 ns |  0.030 ns |  0.85 |    0.00 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    22.26 ns |  0.025 ns |  0.024 ns |  1.19 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    17.33 ns |  0.179 ns |  0.167 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    13.61 ns |  0.017 ns |  0.015 ns |  0.73 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **3,489.42 ns** | **55.667 ns** | **52.071 ns** |  **1.00** |    **0.02** | **0.0153** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  | 3,262.78 ns | 20.412 ns | 19.094 ns |  0.94 |    0.01 | 0.0114 |     232 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 3,473.58 ns | 36.750 ns | 34.376 ns |  1.00 |    0.02 | 0.0229 |     480 B |        1.36 |
| ValueTaskSource_Action             | True    | True  | 3,227.40 ns | 25.459 ns | 23.814 ns |  0.93 |    0.01 | 0.0153 |     310 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 3,402.90 ns | 31.211 ns | 29.195 ns |  0.98 |    0.02 | 0.0229 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  | 3,275.99 ns | 34.637 ns | 32.399 ns |  0.94 |    0.02 | 0.0114 |     240 B |        0.68 |
| Instance_ValueTaskHandler          | True    | True  | 3,214.61 ns | 31.156 ns | 29.144 ns |  0.92 |    0.02 | 0.0114 |     255 B |        0.72 |
