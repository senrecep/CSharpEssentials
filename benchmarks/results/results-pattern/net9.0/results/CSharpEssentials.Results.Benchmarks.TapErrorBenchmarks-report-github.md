```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **22.11 ns** |  **0.137 ns** |  **0.121 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | False |    40.56 ns |  0.260 ns |  0.244 ns |  1.83 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | False   | False |    53.57 ns |  0.166 ns |  0.147 ns |  2.42 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    19.93 ns |  0.220 ns |  0.206 ns |  0.90 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    48.50 ns |  0.043 ns |  0.040 ns |  2.19 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | False   | False |    29.70 ns |  0.041 ns |  0.036 ns |  1.34 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,369.21 ns** |  **5.872 ns** |  **5.492 ns** |  **1.00** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | True  | 1,620.26 ns | 11.529 ns | 10.221 ns |  1.18 | 0.0286 |     478 B |        1.36 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,638.67 ns | 13.318 ns | 12.457 ns |  1.20 | 0.0324 |     566 B |        1.61 |
| Instance_TaskHandler               | False   | True  |   843.60 ns |  5.511 ns |  4.885 ns |  0.62 | 0.0134 |     239 B |        0.68 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,658.66 ns | 13.725 ns | 12.167 ns |  1.21 | 0.0324 |     561 B |        1.59 |
| Instance_ValueTaskHandler          | False   | True  |   855.39 ns |  2.544 ns |  2.124 ns |  0.62 | 0.0143 |     255 B |        0.72 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **18.56 ns** |  **0.083 ns** |  **0.069 ns** |  **1.00** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | False |    36.30 ns |  0.170 ns |  0.159 ns |  1.96 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | True    | False |    51.57 ns |  0.121 ns |  0.113 ns |  2.78 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    18.17 ns |  0.080 ns |  0.074 ns |  0.98 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    44.63 ns |  0.062 ns |  0.058 ns |  2.40 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |    27.39 ns |  0.005 ns |  0.004 ns |  1.48 |      - |         - |        0.00 |
|                                    |         |       |             |           |           |       |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **851.63 ns** |  **4.822 ns** |  **4.511 ns** |  **1.00** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | True  |   860.53 ns |  3.413 ns |  3.192 ns |  1.01 | 0.0191 |     319 B |        1.25 |
| ValueTaskSource_TaskHandler        | True    | True  |   871.24 ns |  7.620 ns |  7.128 ns |  1.02 | 0.0238 |     406 B |        1.59 |
| Instance_TaskHandler               | True    | True  |    18.15 ns |  0.152 ns |  0.143 ns |  0.02 | 0.0048 |      80 B |        0.31 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   865.88 ns |  4.346 ns |  4.065 ns |  1.02 | 0.0181 |     310 B |        1.22 |
| Instance_ValueTaskHandler          | True    | True  |    27.39 ns |  0.009 ns |  0.009 ns |  0.03 |      - |         - |        0.00 |
