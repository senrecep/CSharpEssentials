```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **12.13 ns** |  **0.018 ns** |  **0.015 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    11.45 ns |  0.008 ns |  0.007 ns |  0.94 |    0.00 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    26.28 ns |  0.015 ns |  0.014 ns |  2.17 |    0.00 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    29.46 ns |  0.100 ns |  0.094 ns |  2.43 |    0.01 | 0.0033 |      56 B |          NA |
| Instance_TaskHandler               | False   | False |    22.58 ns |  0.105 ns |  0.093 ns |  1.86 |    0.01 | 0.0033 |      56 B |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    41.54 ns |  0.087 ns |  0.077 ns |  3.43 |    0.01 | 0.0033 |      56 B |          NA |
| Instance_ValueTaskHandler          | False   | False |    24.29 ns |  0.140 ns |  0.131 ns |  2.00 |    0.01 | 0.0033 |      56 B |          NA |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **3,485.75 ns** | **42.050 ns** | **39.333 ns** |  **1.00** |    **0.02** | **0.0229** |     **384 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  | 3,192.98 ns | 32.897 ns | 30.772 ns |  0.92 |    0.01 | 0.0153 |     295 B |        0.77 |
| TaskSource_TaskHandler             | False   | True  | 3,510.72 ns | 42.478 ns | 39.734 ns |  1.01 |    0.02 | 0.0305 |     552 B |        1.44 |
| ValueTaskSource_SyncHandler        | False   | True  | 3,198.27 ns | 25.008 ns | 23.393 ns |  0.92 |    0.01 | 0.0191 |     358 B |        0.93 |
| Instance_TaskHandler               | False   | True  | 3,255.47 ns | 31.831 ns | 29.775 ns |  0.93 |    0.01 | 0.0153 |     296 B |        0.77 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 3,487.55 ns | 35.314 ns | 31.305 ns |  1.00 |    0.01 | 0.0305 |     632 B |        1.65 |
| Instance_ValueTaskHandler          | False   | True  | 3,298.30 ns | 27.826 ns | 26.028 ns |  0.95 |    0.01 | 0.0153 |     311 B |        0.81 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **22.99 ns** |  **0.190 ns** |  **0.168 ns** |  **1.00** |    **0.01** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.70 ns |  0.151 ns |  0.141 ns |  0.73 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    39.34 ns |  0.452 ns |  0.422 ns |  1.71 |    0.02 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    20.47 ns |  0.006 ns |  0.006 ns |  0.89 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    21.36 ns |  0.338 ns |  0.316 ns |  0.93 |    0.01 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    30.64 ns |  0.047 ns |  0.039 ns |  1.33 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    12.70 ns |  0.054 ns |  0.048 ns |  0.55 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **3,446.41 ns** | **36.004 ns** | **33.678 ns** |  **1.00** |    **0.01** | **0.0153** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  | 3,202.81 ns | 23.922 ns | 22.377 ns |  0.93 |    0.01 | 0.0114 |     240 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 3,501.66 ns | 61.137 ns | 79.495 ns |  1.02 |    0.02 | 0.0229 |     496 B |        1.51 |
| ValueTaskSource_SyncHandler        | True    | True  | 3,229.11 ns | 25.665 ns | 22.752 ns |  0.94 |    0.01 | 0.0153 |     303 B |        0.92 |
| Instance_TaskHandler               | True    | True  | 3,251.98 ns | 28.714 ns | 26.859 ns |  0.94 |    0.01 | 0.0114 |     240 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 3,419.15 ns | 36.556 ns | 32.406 ns |  0.99 |    0.01 | 0.0305 |     576 B |        1.76 |
| Instance_ValueTaskHandler          | True    | True  | 3,266.72 ns | 24.915 ns | 23.306 ns |  0.95 |    0.01 | 0.0114 |     255 B |        0.78 |
