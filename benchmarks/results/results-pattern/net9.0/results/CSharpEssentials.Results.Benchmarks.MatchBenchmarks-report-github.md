```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **15.69 ns** |  **0.009 ns** |  **0.008 ns** |  **1.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    13.08 ns |  0.008 ns |  0.007 ns |  0.83 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    30.43 ns |  0.029 ns |  0.026 ns |  1.94 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    28.52 ns |  0.013 ns |  0.013 ns |  1.82 |      - |         - |          NA |
| Instance_TaskHandler               | False   | False |    14.87 ns |  0.008 ns |  0.007 ns |  0.95 |      - |         - |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    40.40 ns |  0.085 ns |  0.076 ns |  2.57 |      - |         - |          NA |
| Instance_ValueTaskHandler          | False   | False |    23.24 ns |  0.011 ns |  0.010 ns |  1.48 |      - |         - |          NA |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,393.15 ns** |  **4.815 ns** |  **4.504 ns** |  **1.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   830.86 ns |  6.233 ns |  5.525 ns |  0.60 | 0.0134 |     238 B |        0.73 |
| TaskSource_TaskHandler             | False   | True  | 1,598.21 ns |  7.092 ns |  6.633 ns |  1.15 | 0.0286 |     492 B |        1.50 |
| ValueTaskSource_SyncHandler        | False   | True  |   857.73 ns |  2.412 ns |  2.014 ns |  0.62 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | False   | True  |   832.77 ns |  4.618 ns |  4.320 ns |  0.60 | 0.0134 |     238 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,633.12 ns |  9.801 ns |  9.168 ns |  1.17 | 0.0343 |     572 B |        1.74 |
| Instance_ValueTaskHandler          | False   | True  |   849.01 ns |  5.735 ns |  5.084 ns |  0.61 | 0.0143 |     255 B |        0.78 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **25.25 ns** |  **0.163 ns** |  **0.152 ns** |  **1.00** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    17.91 ns |  0.156 ns |  0.138 ns |  0.71 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    42.13 ns |  0.228 ns |  0.213 ns |  1.67 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    26.60 ns |  0.020 ns |  0.018 ns |  1.05 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    24.59 ns |  0.246 ns |  0.230 ns |  0.97 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    38.48 ns |  0.044 ns |  0.039 ns |  1.52 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    22.89 ns |  0.008 ns |  0.007 ns |  0.91 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,406.34 ns** | **11.825 ns** |  **9.875 ns** |  **1.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   835.24 ns |  3.625 ns |  3.027 ns |  0.59 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,599.57 ns |  6.934 ns |  5.790 ns |  1.14 | 0.0286 |     494 B |        1.51 |
| ValueTaskSource_SyncHandler        | True    | True  |   847.99 ns |  7.265 ns |  6.796 ns |  0.60 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   841.16 ns |  3.064 ns |  2.866 ns |  0.60 | 0.0134 |     239 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,636.97 ns | 11.944 ns | 11.172 ns |  1.16 | 0.0343 |     572 B |        1.74 |
| Instance_ValueTaskHandler          | True    | True  |   860.14 ns |  4.192 ns |  3.921 ns |  0.61 | 0.0143 |     255 B |        0.78 |
