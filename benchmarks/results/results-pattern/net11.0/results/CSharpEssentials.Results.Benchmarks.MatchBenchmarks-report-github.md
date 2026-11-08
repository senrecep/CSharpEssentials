```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Median      | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **12.64 ns** |  **0.014 ns** |  **0.013 ns** |    **12.65 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| TaskSource_SyncHandler             | False   | False |    11.49 ns |  0.019 ns |  0.018 ns |    11.49 ns |  0.91 |    0.00 |      - |         - |          NA |
| TaskSource_TaskHandler             | False   | False |    26.29 ns |  0.024 ns |  0.021 ns |    26.29 ns |  2.08 |    0.00 |      - |         - |          NA |
| ValueTaskSource_SyncHandler        | False   | False |    20.40 ns |  0.086 ns |  0.080 ns |    20.39 ns |  1.61 |    0.01 |      - |         - |          NA |
| Instance_TaskHandler               | False   | False |    12.22 ns |  0.061 ns |  0.051 ns |    12.20 ns |  0.97 |    0.00 |      - |         - |          NA |
| ValueTaskSource_ValueTaskHandler   | False   | False |    31.53 ns |  0.021 ns |  0.019 ns |    31.52 ns |  2.49 |    0.00 |      - |         - |          NA |
| Instance_ValueTaskHandler          | False   | False |    12.87 ns |  0.014 ns |  0.012 ns |    12.87 ns |  1.02 |    0.00 |      - |         - |          NA |
|                                    |         |       |             |           |           |             |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,023.72 ns** | **20.417 ns** | **56.915 ns** | **1,022.03 ns** |  **1.00** |    **0.08** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | False   | True  |   567.49 ns |  8.498 ns |  7.096 ns |   567.54 ns |  0.56 |    0.03 | 0.0134 |     238 B |        0.73 |
| TaskSource_TaskHandler             | False   | True  | 1,271.30 ns | 25.441 ns | 66.576 ns | 1,241.60 ns |  1.25 |    0.09 | 0.0267 |     490 B |        1.49 |
| ValueTaskSource_SyncHandler        | False   | True  |   665.05 ns | 12.233 ns | 10.844 ns |   662.62 ns |  0.65 |    0.04 | 0.0172 |     302 B |        0.92 |
| Instance_TaskHandler               | False   | True  |   619.91 ns | 12.357 ns | 33.619 ns |   615.97 ns |  0.61 |    0.05 | 0.0134 |     238 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,234.38 ns | 24.419 ns | 46.460 ns | 1,232.02 ns |  1.21 |    0.08 | 0.0324 |     569 B |        1.73 |
| Instance_ValueTaskHandler          | False   | True  |   611.15 ns | 11.589 ns | 12.400 ns |   612.45 ns |  0.60 |    0.03 | 0.0143 |     255 B |        0.78 |
|                                    |         |       |             |           |           |             |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **20.91 ns** |  **0.396 ns** |  **0.371 ns** |    **21.01 ns** |  **1.00** |    **0.02** | **0.0086** |     **144 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | False |    15.67 ns |  0.157 ns |  0.132 ns |    15.67 ns |  0.75 |    0.01 | 0.0043 |      72 B |        0.50 |
| TaskSource_TaskHandler             | True    | False |    36.63 ns |  0.763 ns |  1.642 ns |    36.62 ns |  1.75 |    0.08 | 0.0129 |     216 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | False |    20.81 ns |  0.040 ns |  0.031 ns |    20.81 ns |  1.00 |    0.02 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    20.46 ns |  0.439 ns |  0.803 ns |    20.35 ns |  0.98 |    0.04 | 0.0086 |     144 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    30.44 ns |  0.055 ns |  0.049 ns |    30.42 ns |  1.46 |    0.03 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    11.87 ns |  0.018 ns |  0.017 ns |    11.87 ns |  0.57 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |             |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **946.67 ns** |  **8.357 ns** |  **7.817 ns** |   **949.40 ns** |  **1.00** |    **0.01** | **0.0191** |     **328 B** |        **1.00** |
| TaskSource_SyncHandler             | True    | True  |   535.69 ns |  4.600 ns |  4.303 ns |   534.64 ns |  0.57 |    0.01 | 0.0134 |     238 B |        0.73 |
| TaskSource_TaskHandler             | True    | True  | 1,080.17 ns |  9.335 ns |  8.275 ns | 1,078.45 ns |  1.14 |    0.01 | 0.0267 |     492 B |        1.50 |
| ValueTaskSource_SyncHandler        | True    | True  |   614.72 ns |  7.623 ns |  7.131 ns |   613.69 ns |  0.65 |    0.01 | 0.0172 |     301 B |        0.92 |
| Instance_TaskHandler               | True    | True  |   552.59 ns |  5.095 ns |  4.766 ns |   552.65 ns |  0.58 |    0.01 | 0.0134 |     238 B |        0.73 |
| ValueTaskSource_ValueTaskHandler   | True    | True  | 1,159.73 ns | 22.866 ns | 38.829 ns | 1,173.44 ns |  1.23 |    0.04 | 0.0305 |     569 B |        1.73 |
| Instance_ValueTaskHandler          | True    | True  |   615.04 ns |  6.372 ns |  5.961 ns |   614.71 ns |  0.65 |    0.01 | 0.0143 |     253 B |        0.77 |
