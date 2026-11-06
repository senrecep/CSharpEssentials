```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **13.10 ns** |  **0.098 ns** |  **0.092 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    11.25 ns |  0.012 ns |  0.010 ns |  0.86 |    0.01 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    26.29 ns |  0.016 ns |  0.014 ns |  2.01 |    0.01 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    35.32 ns |  0.090 ns |  0.080 ns |  2.70 |    0.02 | 0.0033 |      56 B |          NA |
| Instance_TaskHandler               | False   | False |    24.22 ns |  0.085 ns |  0.075 ns |  1.85 |    0.01 | 0.0033 |      56 B |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    45.48 ns |  0.102 ns |  0.095 ns |  3.47 |    0.02 | 0.0033 |      56 B |          NA |
| Instance_ValueTaskHandler          | False   | False |    25.75 ns |  0.089 ns |  0.083 ns |  1.97 |    0.01 | 0.0033 |      56 B |          NA |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,398.95 ns** |  **7.688 ns** |  **6.815 ns** |  **1.00** |    **0.01** | **0.0229** |     **384 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   843.88 ns |  5.884 ns |  5.504 ns |  0.60 |    0.00 | 0.0172 |     295 B |        0.77 |
| TaskSource_TaskHandler             | False   | True  | 1,619.21 ns |  4.301 ns |  3.812 ns |  1.16 |    0.01 | 0.0324 |     549 B |        1.43 |
| ValueTaskSource_SyncHandler        | False   | True  |   855.62 ns |  4.162 ns |  3.893 ns |  0.61 |    0.00 | 0.0210 |     359 B |        0.93 |
| Instance_TaskHandler               | False   | True  |   839.84 ns |  4.561 ns |  4.267 ns |  0.60 |    0.00 | 0.0172 |     295 B |        0.77 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,673.37 ns |  9.769 ns |  9.138 ns |  1.20 |    0.01 | 0.0362 |     629 B |        1.64 |
| Instance_ValueTaskHandler          | False   | True  |   847.43 ns |  5.090 ns |  4.250 ns |  0.61 |    0.00 | 0.0181 |     311 B |        0.81 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **21.97 ns** |  **0.261 ns** |  **0.232 ns** |  **1.00** |    **0.01** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    16.46 ns |  0.167 ns |  0.148 ns |  0.75 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    39.71 ns |  0.241 ns |  0.226 ns |  1.81 |    0.02 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    20.53 ns |  0.026 ns |  0.022 ns |  0.93 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    21.66 ns |  0.162 ns |  0.151 ns |  0.99 |    0.01 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    31.72 ns |  0.030 ns |  0.028 ns |  1.44 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    14.53 ns |  0.101 ns |  0.089 ns |  0.66 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **1,401.92 ns** |  **4.606 ns** |  **4.308 ns** |  **1.00** |    **0.00** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   835.51 ns |  4.876 ns |  4.561 ns |  0.60 |    0.00 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,581.20 ns |  8.422 ns |  7.878 ns |  1.13 |    0.01 | 0.0267 |     493 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | True  |   856.32 ns |  3.230 ns |  3.021 ns |  0.61 |    0.00 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   839.10 ns |  3.018 ns |  2.823 ns |  0.60 |    0.00 | 0.0134 |     239 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,624.19 ns | 16.228 ns | 15.180 ns |  1.16 |    0.01 | 0.0305 |     571 B |        1.74 |
| Instance_ValueTaskHandler          | True    | True  |   856.47 ns |  3.776 ns |  3.532 ns |  0.61 |    0.00 | 0.0143 |     255 B |        0.78 |
