```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **16.26 ns** |  **0.019 ns** |  **0.017 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    13.09 ns |  0.011 ns |  0.010 ns |  0.81 |    0.00 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    32.68 ns |  0.079 ns |  0.074 ns |  2.01 |    0.00 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    45.67 ns |  0.118 ns |  0.111 ns |  2.81 |    0.01 | 0.0033 |      56 B |          NA |
| Instance_TaskHandler               | False   | False |    29.44 ns |  0.148 ns |  0.139 ns |  1.81 |    0.01 | 0.0033 |      56 B |          NA |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,440.37 ns** |  **9.132 ns** |  **8.542 ns** |  **1.00** |    **0.01** | **0.0229** |     **384 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   861.79 ns |  7.051 ns |  6.596 ns |  0.60 |    0.01 | 0.0172 |     295 B |        0.77 |
| TaskSource_TaskHandler             | False   | True  | 1,639.41 ns |  7.419 ns |  6.940 ns |  1.14 |    0.01 | 0.0324 |     549 B |        1.43 |
| ValueTaskSource_SyncHandler        | False   | True  |   901.33 ns |  5.552 ns |  5.194 ns |  0.63 |    0.01 | 0.0210 |     358 B |        0.93 |
| Instance_TaskHandler               | False   | True  |   869.99 ns |  4.568 ns |  4.273 ns |  0.60 |    0.00 | 0.0172 |     295 B |        0.77 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **26.86 ns** |  **0.282 ns** |  **0.235 ns** |  **1.00** |    **0.01** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    19.40 ns |  0.080 ns |  0.075 ns |  0.72 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    49.57 ns |  0.238 ns |  0.223 ns |  1.85 |    0.02 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    28.46 ns |  0.044 ns |  0.039 ns |  1.06 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    26.08 ns |  0.198 ns |  0.186 ns |  0.97 |    0.01 | 0.0086 |     144 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,409.34 ns** |  **6.638 ns** |  **6.209 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   849.73 ns |  5.958 ns |  4.652 ns |  0.60 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,666.87 ns | 14.830 ns | 13.872 ns |  1.18 |    0.01 | 0.0267 |     494 B |        1.51 |
| ValueTaskSource_SyncHandler        | True    | True  |   881.58 ns |  4.108 ns |  3.843 ns |  0.63 |    0.00 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   837.41 ns |  7.044 ns |  6.244 ns |  0.59 |    0.00 | 0.0134 |     239 B |        0.73 |
