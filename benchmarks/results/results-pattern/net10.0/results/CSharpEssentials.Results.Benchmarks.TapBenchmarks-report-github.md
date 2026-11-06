```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **17.24 ns** | **0.212 ns** | **0.199 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    16.78 ns | 0.157 ns | 0.147 ns |  0.97 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    24.18 ns | 0.156 ns | 0.146 ns |  1.40 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    38.11 ns | 0.128 ns | 0.120 ns |  2.21 |    0.03 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    36.36 ns | 0.133 ns | 0.124 ns |  2.11 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    21.83 ns | 0.210 ns | 0.186 ns |  1.27 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    14.75 ns | 0.119 ns | 0.111 ns |  0.86 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **842.46 ns** | **3.486 ns** | **3.261 ns** |  **1.00** |    **0.01** | **0.0181** |     **311 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   863.76 ns | 3.517 ns | 3.290 ns |  1.03 |    0.01 | 0.0172 |     287 B |        0.92 |
| TaskSource_TaskHandler             | False   | True  |   862.47 ns | 4.836 ns | 4.524 ns |  1.02 |    0.01 | 0.0219 |     375 B |        1.21 |
| ValueTaskSource_Action             | False   | True  |   852.73 ns | 6.429 ns | 6.014 ns |  1.01 |    0.01 | 0.0219 |     367 B |        1.18 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   867.56 ns | 5.679 ns | 5.034 ns |  1.03 |    0.01 | 0.0238 |     399 B |        1.28 |
| Instance_TaskHandler               | False   | True  |    21.99 ns | 0.272 ns | 0.241 ns |  0.03 |    0.00 | 0.0081 |     136 B |        0.44 |
| Instance_ValueTaskHandler          | False   | True  |    16.51 ns | 0.095 ns | 0.089 ns |  0.02 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **19.51 ns** | **0.093 ns** | **0.087 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    16.87 ns | 0.176 ns | 0.165 ns |  0.86 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    37.34 ns | 0.166 ns | 0.156 ns |  1.91 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    15.49 ns | 0.031 ns | 0.026 ns |  0.79 |    0.00 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    22.53 ns | 0.020 ns | 0.018 ns |  1.16 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    18.08 ns | 0.089 ns | 0.084 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    12.86 ns | 0.005 ns | 0.004 ns |  0.66 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,382.15 ns** | **5.968 ns** | **5.290 ns** |  **1.00** |    **0.01** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   834.98 ns | 3.782 ns | 3.538 ns |  0.60 |    0.00 | 0.0134 |     232 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,613.96 ns | 6.262 ns | 5.857 ns |  1.17 |    0.01 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_Action             | True    | True  |   860.19 ns | 3.730 ns | 3.489 ns |  0.62 |    0.00 | 0.0181 |     310 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,454.61 ns | 9.069 ns | 8.483 ns |  1.05 |    0.01 | 0.0248 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   839.69 ns | 3.082 ns | 2.883 ns |  0.61 |    0.00 | 0.0134 |     239 B |        0.68 |
| Instance_ValueTaskHandler          | True    | True  |   859.54 ns | 4.624 ns | 4.325 ns |  0.62 |    0.00 | 0.0143 |     255 B |        0.72 |
