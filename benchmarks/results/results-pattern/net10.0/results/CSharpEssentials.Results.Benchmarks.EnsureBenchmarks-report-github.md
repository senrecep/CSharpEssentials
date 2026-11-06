```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **27.89 ns** |  **0.119 ns** | **0.111 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    19.60 ns |  0.192 ns | 0.179 ns |  0.70 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    36.45 ns |  0.296 ns | 0.277 ns |  1.31 |    0.01 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    31.69 ns |  0.220 ns | 0.206 ns |  1.14 |    0.01 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    70.19 ns |  0.178 ns | 0.158 ns |  2.52 |    0.01 | 0.0033 |      56 B |        0.70 |
| Instance_ValueTaskPredicate          | False   | False |    42.97 ns |  0.105 ns | 0.098 ns |  1.54 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **845.48 ns** |  **2.980 ns** | **2.787 ns** |  **1.00** |    **0.00** | **0.0181** |     **311 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   858.50 ns |  3.555 ns | 3.325 ns |  1.02 |    0.01 | 0.0191 |     319 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   873.95 ns |  7.015 ns | 6.561 ns |  1.03 |    0.01 | 0.0238 |     407 B |        1.31 |
| Instance_TaskPredicate               | False   | True  |    31.37 ns |  0.375 ns | 0.351 ns |  0.04 |    0.00 | 0.0081 |     136 B |        0.44 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  |   865.84 ns |  5.159 ns | 4.573 ns |  1.02 |    0.01 | 0.0238 |     399 B |        1.28 |
| Instance_ValueTaskPredicate          | False   | True  |    48.64 ns |  0.116 ns | 0.109 ns |  0.06 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **20.38 ns** |  **0.148 ns** | **0.263 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    18.89 ns |  0.123 ns | 0.115 ns |  0.93 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    40.98 ns |  0.479 ns | 0.448 ns |  2.01 |    0.03 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    20.67 ns |  0.078 ns | 0.073 ns |  1.01 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    53.65 ns |  0.166 ns | 0.156 ns |  2.63 |    0.03 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    36.91 ns |  0.100 ns | 0.094 ns |  1.81 |    0.02 |      - |         - |        0.00 |
|                                      |         |       |             |           |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,373.91 ns** |  **5.550 ns** | **5.191 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   842.66 ns |  4.934 ns | 4.615 ns |  0.61 |    0.00 | 0.0153 |     262 B |        0.74 |
| TaskSource_TaskPredicate             | True    | True  | 1,623.92 ns | 10.339 ns | 9.671 ns |  1.18 |    0.01 | 0.0324 |     541 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   846.72 ns |  6.107 ns | 5.713 ns |  0.62 |    0.00 | 0.0153 |     271 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 1,682.62 ns |  7.542 ns | 7.055 ns |  1.22 |    0.01 | 0.0381 |     634 B |        1.80 |
| Instance_ValueTaskPredicate          | True    | True  |   855.20 ns |  4.898 ns | 4.581 ns |  0.62 |    0.00 | 0.0172 |     295 B |        0.84 |
