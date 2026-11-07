```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|---------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **19.50 ns** |  **0.106 ns** | **0.094 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    21.61 ns |  0.127 ns | 0.119 ns |  1.11 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    29.73 ns |  0.173 ns | 0.162 ns |  1.52 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    30.77 ns |  0.037 ns | 0.035 ns |  1.58 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    30.79 ns |  0.019 ns | 0.017 ns |  1.58 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |    10.61 ns |  0.059 ns | 0.055 ns |  0.54 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |          |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **853.06 ns** |  **4.435 ns** | **4.148 ns** |  **1.00** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   845.20 ns |  5.073 ns | 4.745 ns |  0.99 | 0.0134 |     231 B |        0.91 |
| TaskSource_TaskHandler             | False   | True  |   844.18 ns |  3.396 ns | 3.011 ns |  0.99 | 0.0191 |     319 B |        1.25 |
| ValueTaskSource_Action             | False   | True  |   879.95 ns |  6.752 ns | 6.315 ns |  1.03 | 0.0181 |     311 B |        1.22 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   863.97 ns |  5.554 ns | 5.195 ns |  1.01 | 0.0200 |     342 B |        1.34 |
| Instance_TaskHandler               | False   | True  |    10.49 ns |  0.083 ns | 0.074 ns |  0.01 | 0.0048 |      80 B |        0.31 |
|                                    |         |       |             |           |          |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **22.22 ns** |  **0.127 ns** | **0.106 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    21.92 ns |  0.102 ns | 0.095 ns |  0.99 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    41.12 ns |  0.243 ns | 0.227 ns |  1.85 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    29.59 ns |  0.036 ns | 0.033 ns |  1.33 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    35.82 ns |  0.175 ns | 0.164 ns |  1.61 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    19.74 ns |  0.108 ns | 0.101 ns |  0.89 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |          |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,410.27 ns** | **10.374 ns** | **9.704 ns** |  **1.00** | **0.0191** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   851.23 ns |  3.988 ns | 3.535 ns |  0.60 | 0.0134 |     232 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,606.63 ns |  8.462 ns | 7.066 ns |  1.14 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_Action             | True    | True  |   864.85 ns |  5.706 ns | 5.338 ns |  0.61 | 0.0181 |     311 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,551.05 ns | 10.195 ns | 9.537 ns |  1.10 | 0.0248 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   826.85 ns |  5.008 ns | 4.685 ns |  0.59 | 0.0134 |     239 B |        0.68 |
