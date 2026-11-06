```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **16.71 ns** |  **0.184 ns** |  **0.172 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |    16.75 ns |  0.100 ns |  0.089 ns |  1.00 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    17.39 ns |  0.174 ns |  0.163 ns |  1.04 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_SyncHandler        | False   | False |    28.42 ns |  0.085 ns |  0.079 ns |  1.70 |    0.02 | 0.0033 |      56 B |        0.70 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    28.11 ns |  0.083 ns |  0.077 ns |  1.68 |    0.02 | 0.0033 |      56 B |        0.70 |
| Instance_TaskHandler               | False   | False |    21.71 ns |  0.237 ns |  0.222 ns |  1.30 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_ValueTaskHandler          | False   | False |    14.55 ns |  0.309 ns |  0.391 ns |  0.87 |    0.02 | 0.0033 |      56 B |        0.70 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **3,281.63 ns** | **26.644 ns** | **24.923 ns** | **1.000** |    **0.01** | **0.0153** |     **296 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  | 3,241.86 ns | 22.745 ns | 21.275 ns | 0.988 |    0.01 | 0.0153 |     288 B |        0.97 |
| TaskSource_TaskHandler             | False   | True  | 3,225.04 ns | 29.227 ns | 27.339 ns | 0.983 |    0.01 | 0.0153 |     312 B |        1.05 |
| ValueTaskSource_SyncHandler        | False   | True  | 3,187.86 ns | 15.057 ns | 13.347 ns | 0.971 |    0.01 | 0.0191 |     366 B |        1.24 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 3,198.05 ns | 29.894 ns | 27.963 ns | 0.975 |    0.01 | 0.0229 |     382 B |        1.29 |
| Instance_TaskHandler               | False   | True  |    21.85 ns |  0.358 ns |  0.335 ns | 0.007 |    0.00 | 0.0081 |     136 B |        0.46 |
| Instance_ValueTaskHandler          | False   | True  |    14.75 ns |  0.174 ns |  0.162 ns | 0.004 |    0.00 | 0.0033 |      56 B |        0.19 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **21.80 ns** |  **0.277 ns** |  **0.259 ns** |  **1.00** |    **0.02** | **0.0091** |     **152 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.49 ns |  0.190 ns |  0.168 ns |  0.76 |    0.01 | 0.0048 |      80 B |        0.53 |
| TaskSource_TaskHandler             | True    | False |    23.85 ns |  0.401 ns |  0.313 ns |  1.09 |    0.02 | 0.0091 |     152 B |        1.00 |
| ValueTaskSource_SyncHandler        | True    | False |    17.59 ns |  0.075 ns |  0.070 ns |  0.81 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    22.60 ns |  0.066 ns |  0.062 ns |  1.04 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    21.20 ns |  0.430 ns |  0.402 ns |  0.97 |    0.02 | 0.0091 |     152 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    12.67 ns |  0.043 ns |  0.038 ns |  0.58 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **3,453.30 ns** | **46.526 ns** | **43.520 ns** |  **1.00** |    **0.02** | **0.0153** |     **336 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  | 3,255.49 ns | 28.523 ns | 26.680 ns |  0.94 |    0.01 | 0.0114 |     232 B |        0.69 |
| TaskSource_TaskHandler             | True    | True  | 3,408.61 ns | 36.953 ns | 34.566 ns |  0.99 |    0.02 | 0.0153 |     352 B |        1.05 |
| ValueTaskSource_SyncHandler        | True    | True  | 3,187.03 ns | 24.620 ns | 23.029 ns |  0.92 |    0.01 | 0.0153 |     310 B |        0.92 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 3,461.29 ns | 39.619 ns | 35.121 ns |  1.00 |    0.02 | 0.0229 |     432 B |        1.29 |
| Instance_TaskHandler               | True    | True  | 3,201.77 ns | 23.764 ns | 22.229 ns |  0.93 |    0.01 | 0.0114 |     224 B |        0.67 |
| Instance_ValueTaskHandler          | True    | True  | 3,304.71 ns | 17.957 ns | 16.797 ns |  0.96 |    0.01 | 0.0114 |     247 B |        0.74 |
