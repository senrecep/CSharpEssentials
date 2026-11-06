```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **23.80 ns** |  **0.143 ns** |  **0.133 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | False |    44.38 ns |  0.244 ns |  0.228 ns |  1.86 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | False   | False |    67.12 ns |  0.220 ns |  0.206 ns |  2.82 |    0.02 | 0.0081 |     136 B |        1.70 |
| Instance_TaskHandler               | False   | False |    36.32 ns |  0.245 ns |  0.229 ns |  1.53 |    0.01 | 0.0081 |     136 B |        1.70 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  | **1,425.01 ns** |  **4.640 ns** |  **4.341 ns** |  **1.00** |    **0.00** | **0.0229** |     **408 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | True  | 1,652.66 ns | 12.888 ns | 11.425 ns |  1.16 |    0.01 | 0.0305 |     534 B |        1.31 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,716.88 ns | 10.947 ns | 10.240 ns |  1.20 |    0.01 | 0.0362 |     623 B |        1.53 |
| Instance_TaskHandler               | False   | True  |   855.23 ns |  4.427 ns |  4.141 ns |  0.60 |    0.00 | 0.0172 |     296 B |        0.73 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **20.25 ns** |  **0.116 ns** |  **0.103 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | False |    39.92 ns |  0.192 ns |  0.170 ns |  1.97 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | True    | False |    48.59 ns |  0.131 ns |  0.123 ns |  2.40 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    21.89 ns |  0.126 ns |  0.112 ns |  1.08 |    0.01 | 0.0048 |      80 B |        1.00 |
|                                    |         |       |             |           |           |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **850.43 ns** |  **1.995 ns** |  **1.557 ns** |  **1.00** |    **0.00** | **0.0143** |     **255 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | True  |   882.11 ns |  4.818 ns |  3.762 ns |  1.04 |    0.00 | 0.0191 |     319 B |        1.25 |
| ValueTaskSource_TaskHandler        | True    | True  |   913.65 ns |  5.058 ns |  4.731 ns |  1.07 |    0.01 | 0.0229 |     406 B |        1.59 |
| Instance_TaskHandler               | True    | True  |    18.76 ns |  0.187 ns |  0.175 ns |  0.02 |    0.00 | 0.0048 |      80 B |        0.31 |
