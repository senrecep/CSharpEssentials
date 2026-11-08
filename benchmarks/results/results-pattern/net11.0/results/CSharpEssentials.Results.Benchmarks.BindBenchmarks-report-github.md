```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-----------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |  **15.026 ns** |  **0.1214 ns** |  **0.0948 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | False |  17.036 ns |  0.1443 ns |  0.1350 ns |  1.13 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |  16.956 ns |  0.1243 ns |  0.1162 ns |  1.13 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |  22.748 ns |  0.0137 ns |  0.0122 ns |  1.51 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |   8.781 ns |  0.0939 ns |  0.0833 ns |  0.58 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |   4.232 ns |  0.0072 ns |  0.0060 ns |  0.28 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **548.423 ns** |  **7.9827 ns** |  **7.4670 ns** | **1.000** |    **0.02** | **0.0124** |     **223 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  | 551.419 ns |  5.6081 ns |  4.9715 ns | 1.006 |    0.02 | 0.0134 |     239 B |        1.07 |
| TaskSource_TaskHandler             | False   | True  | 551.081 ns |  7.4093 ns |  6.9307 ns | 1.005 |    0.02 | 0.0143 |     246 B |        1.10 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 632.600 ns | 12.2978 ns | 15.5528 ns | 1.154 |    0.03 | 0.0200 |     340 B |        1.52 |
| Instance_TaskHandler               | False   | True  |   8.769 ns |  0.1242 ns |  0.1101 ns | 0.016 |    0.00 | 0.0048 |      80 B |        0.36 |
| Instance_ValueTaskHandler          | False   | True  |   3.170 ns |  0.0058 ns |  0.0054 ns | 0.006 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |  **20.827 ns** |  **0.3041 ns** |  **0.2845 ns** |  **1.00** |    **0.02** | **0.0095** |     **160 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |  16.647 ns |  0.1908 ns |  0.1784 ns |  0.80 |    0.01 | 0.0048 |      80 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |  23.081 ns |  0.1889 ns |  0.1674 ns |  1.11 |    0.02 | 0.0095 |     160 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |  32.096 ns |  0.0245 ns |  0.0218 ns |  1.54 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |   7.257 ns |  0.0967 ns |  0.0904 ns |  0.35 |    0.01 | 0.0048 |      80 B |        0.50 |
| Instance_ValueTaskHandler          | True    | False |   2.830 ns |  0.0036 ns |  0.0034 ns |  0.14 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |            |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  | **903.853 ns** |  **7.3311 ns** |  **6.8575 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  | 547.012 ns |  6.2893 ns |  5.8830 ns |  0.61 |    0.01 | 0.0134 |     239 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 895.201 ns | 16.4932 ns | 15.4278 ns |  0.99 |    0.02 | 0.0191 |     352 B |        1.07 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 980.723 ns | 10.4198 ns |  9.2369 ns |  1.09 |    0.01 | 0.0267 |     464 B |        1.41 |
| Instance_TaskHandler               | True    | True  | 454.062 ns |  7.8364 ns |  7.3302 ns |  0.50 |    0.01 | 0.0062 |     104 B |        0.32 |
| Instance_ValueTaskHandler          | True    | True  | 517.400 ns |  4.6148 ns |  4.3167 ns |  0.57 |    0.01 | 0.0067 |     120 B |        0.37 |
