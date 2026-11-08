```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **22.67 ns** |  **0.118 ns** |  **0.105 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | False |    42.48 ns |  0.176 ns |  0.165 ns |  1.87 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | False   | False |    48.90 ns |  0.083 ns |  0.074 ns |  2.16 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    20.52 ns |  0.078 ns |  0.065 ns |  0.91 |    0.00 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,407.13 ns** |  **7.512 ns** |  **7.027 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | True  | 1,598.09 ns | 12.758 ns | 11.934 ns |  1.14 |    0.01 | 0.0267 |     478 B |        1.36 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,669.15 ns |  7.930 ns |  6.622 ns |  1.19 |    0.01 | 0.0324 |     566 B |        1.61 |
| Instance_TaskHandler               | False   | True  |   846.44 ns | 14.886 ns | 13.924 ns |  0.60 |    0.01 | 0.0143 |     239 B |        0.68 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **19.59 ns** |  **0.083 ns** |  **0.073 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | False |    38.90 ns |  0.389 ns |  0.364 ns |  1.99 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | True    | False |    44.52 ns |  0.082 ns |  0.073 ns |  2.27 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    17.76 ns |  0.120 ns |  0.112 ns |  0.91 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **836.61 ns** |  **5.404 ns** |  **5.055 ns** |  **1.00** |    **0.01** | **0.0153** |     **255 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | True  |   845.13 ns |  6.906 ns |  6.459 ns |  1.01 |    0.01 | 0.0191 |     319 B |        1.25 |
| ValueTaskSource_TaskHandler        | True    | True  |   888.53 ns |  6.668 ns |  5.911 ns |  1.06 |    0.01 | 0.0238 |     407 B |        1.60 |
| Instance_TaskHandler               | True    | True  |    17.74 ns |  0.130 ns |  0.121 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.31 |
