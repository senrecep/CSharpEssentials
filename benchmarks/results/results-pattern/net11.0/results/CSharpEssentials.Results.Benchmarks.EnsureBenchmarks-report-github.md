```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                               | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **False** |    **16.50 ns** |  **0.221 ns** |  **0.206 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | False |    19.39 ns |  0.140 ns |  0.124 ns |  1.18 |    0.02 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | False   | False |    35.77 ns |  0.302 ns |  0.282 ns |  2.17 |    0.03 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | False   | False |    30.58 ns |  0.385 ns |  0.360 ns |  1.85 |    0.03 | 0.0081 |     136 B |        1.70 |
| ValueTaskSource_ValueTaskPredicate   | False   | False |    59.74 ns |  0.113 ns |  0.094 ns |  3.62 |    0.04 | 0.0033 |      56 B |        0.70 |
| Instance_ValueTaskPredicate          | False   | False |    41.37 ns |  0.084 ns |  0.070 ns |  2.51 |    0.03 | 0.0033 |      56 B |        0.70 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **False**   | **True**  | **3,286.84 ns** | **33.439 ns** | **31.279 ns** | **1.000** |    **0.01** | **0.0153** |     **312 B** |        **1.00** |
| TaskSource_SyncPredicate             | False   | True  | 3,253.73 ns | 23.475 ns | 21.959 ns | 0.990 |    0.01 | 0.0191 |     320 B |        1.03 |
| TaskSource_TaskPredicate             | False   | True  | 3,240.29 ns | 21.506 ns | 19.064 ns | 0.986 |    0.01 | 0.0229 |     408 B |        1.31 |
| Instance_TaskPredicate               | False   | True  |    30.06 ns |  0.622 ns |  0.582 ns | 0.009 |    0.00 | 0.0081 |     136 B |        0.44 |
| ValueTaskSource_ValueTaskPredicate   | False   | True  | 3,348.31 ns | 24.670 ns | 21.869 ns | 1.019 |    0.01 | 0.0229 |     399 B |        1.28 |
| Instance_ValueTaskPredicate          | False   | True  |    39.06 ns |  0.166 ns |  0.155 ns | 0.012 |    0.00 | 0.0033 |      56 B |        0.18 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **False** |    **20.45 ns** |  **0.155 ns** |  **0.145 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | False |    19.30 ns |  0.155 ns |  0.145 ns |  0.94 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskPredicate             | True    | False |    39.88 ns |  0.383 ns |  0.358 ns |  1.95 |    0.02 | 0.0095 |     160 B |        2.00 |
| Instance_TaskPredicate               | True    | False |    20.20 ns |  0.157 ns |  0.147 ns |  0.99 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskPredicate   | True    | False |    53.83 ns |  0.017 ns |  0.016 ns |  2.63 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskPredicate          | True    | False |    33.84 ns |  0.050 ns |  0.047 ns |  1.65 |    0.01 |      - |         - |        0.00 |
|                                      |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskPredicate** | **True**    | **True**  | **3,489.48 ns** | **35.397 ns** | **33.111 ns** |  **1.00** |    **0.01** | **0.0153** |     **352 B** |        **1.00** |
| TaskSource_SyncPredicate             | True    | True  | 3,279.81 ns | 27.450 ns | 25.677 ns |  0.94 |    0.01 | 0.0153 |     264 B |        0.75 |
| TaskSource_TaskPredicate             | True    | True  | 3,491.71 ns | 32.534 ns | 27.167 ns |  1.00 |    0.01 | 0.0305 |     544 B |        1.55 |
| Instance_TaskPredicate               | True    | True  | 3,247.35 ns | 34.961 ns | 32.702 ns |  0.93 |    0.01 | 0.0153 |     272 B |        0.77 |
| ValueTaskSource_ValueTaskPredicate   | True    | True  | 3,534.67 ns | 56.113 ns | 52.489 ns |  1.01 |    0.02 | 0.0381 |     640 B |        1.82 |
| Instance_ValueTaskPredicate          | True    | True  | 3,286.16 ns | 24.941 ns | 22.110 ns |  0.94 |    0.01 | 0.0153 |     295 B |        0.84 |
