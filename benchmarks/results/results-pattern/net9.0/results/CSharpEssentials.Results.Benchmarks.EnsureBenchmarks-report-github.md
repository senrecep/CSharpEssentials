```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **20.52 ns** | **0.130 ns** | **0.122 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    21.28 ns | 0.132 ns | 0.123 ns |  1.04 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    40.10 ns | 0.238 ns | 0.223 ns |  1.95 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    33.80 ns | 0.303 ns | 0.284 ns |  1.65 |    0.02 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    60.66 ns | 0.318 ns | 0.297 ns |  2.96 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_ValueTaskPredicate          | False   | False |    39.89 ns | 0.095 ns | 0.079 ns |  1.94 |    0.01 | 0.0033 |      56 B |        0.70 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  |   **875.69 ns** | **4.454 ns** | **4.166 ns** |  **1.00** |    **0.01** | **0.0181** |     **311 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  |   874.93 ns | 4.621 ns | 4.322 ns |  1.00 |    0.01 | 0.0191 |     319 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  |   883.63 ns | 5.184 ns | 4.849 ns |  1.01 |    0.01 | 0.0238 |     407 B |        1.31 |
| Instance_TaskPredicate               | False   | True  |    33.59 ns | 0.183 ns | 0.172 ns |  0.04 |    0.00 | 0.0081 |     136 B |        0.44 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  |   892.09 ns | 5.678 ns | 5.311 ns |  1.02 |    0.01 | 0.0238 |     399 B |        1.28 |
| Instance_ValueTaskPredicate          | False   | True  |    40.48 ns | 0.201 ns | 0.179 ns |  0.05 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **22.01 ns** | **0.110 ns** | **0.103 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    20.32 ns | 0.240 ns | 0.213 ns |  0.92 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    43.38 ns | 0.533 ns | 0.472 ns |  1.97 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    21.92 ns | 0.170 ns | 0.159 ns |  1.00 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    51.62 ns | 0.062 ns | 0.058 ns |  2.34 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    30.52 ns | 0.054 ns | 0.051 ns |  1.39 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |          |          |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **1,390.77 ns** | **6.538 ns** | **6.116 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  |   847.17 ns | 5.468 ns | 5.114 ns |  0.61 |    0.00 | 0.0153 |     263 B |        0.75 |
| TaskSource_TaskPredicate             | True    | True  | 1,622.20 ns | 7.543 ns | 6.687 ns |  1.17 |    0.01 | 0.0324 |     541 B |        1.54 |
| Instance_TaskPredicate               | True    | True  |   849.18 ns | 4.237 ns | 3.963 ns |  0.61 |    0.00 | 0.0153 |     271 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 1,703.39 ns | 9.694 ns | 9.068 ns |  1.22 |    0.01 | 0.0381 |     636 B |        1.81 |
| Instance_ValueTaskPredicate          | True    | True  |   865.10 ns | 4.677 ns | 4.375 ns |  0.62 |    0.00 | 0.0172 |     295 B |        0.84 |
