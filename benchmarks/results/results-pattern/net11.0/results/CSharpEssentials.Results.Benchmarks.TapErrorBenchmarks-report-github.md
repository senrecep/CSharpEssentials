```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **18.539 ns** |  **0.1281 ns** |  **0.1198 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | False |    34.311 ns |  0.3053 ns |  0.2856 ns |  1.85 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | False   | False |    38.434 ns |  0.2281 ns |  0.2134 ns |  2.07 |    0.02 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | False   | False |    18.083 ns |  0.1143 ns |  0.1069 ns |  0.98 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    33.661 ns |  0.0612 ns |  0.0511 ns |  1.82 |    0.01 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | False   | False |    13.516 ns |  0.0236 ns |  0.0209 ns |  0.73 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **988.241 ns** | **19.7602 ns** | **33.5543 ns** |  **1.00** |    **0.05** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_TaskHandler             | False   | True  | 1,052.423 ns |  7.8231 ns |  7.3177 ns |  1.07 |    0.04 | 0.0286 |     476 B |        1.35 |
| ValueTaskSource_TaskHandler        | False   | True  | 1,095.202 ns | 15.3307 ns | 14.3404 ns |  1.11 |    0.04 | 0.0305 |     565 B |        1.61 |
| Instance_TaskHandler               | False   | True  |   534.665 ns |  4.8617 ns |  4.5476 ns |  0.54 |    0.02 | 0.0134 |     239 B |        0.68 |
| ValueTaskSource_ValueTaskHandler   | False   | True  | 1,113.247 ns | 10.8227 ns |  9.0375 ns |  1.13 |    0.04 | 0.0324 |     560 B |        1.59 |
| Instance_ValueTaskHandler          | False   | True  |   603.553 ns |  5.2325 ns |  4.6385 ns |  0.61 |    0.02 | 0.0143 |     254 B |        0.72 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **15.895 ns** |  **0.1734 ns** |  **0.1622 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | False |    31.503 ns |  0.2569 ns |  0.2403 ns |  1.98 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_TaskHandler        | True    | False |    34.784 ns |  0.2054 ns |  0.1821 ns |  2.19 |    0.02 | 0.0048 |      80 B |        1.00 |
| Instance_TaskHandler               | True    | False |    14.527 ns |  0.0812 ns |  0.0720 ns |  0.91 |    0.01 | 0.0048 |      80 B |        1.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    27.695 ns |  0.0268 ns |  0.0251 ns |  1.74 |    0.02 |      - |         - |        0.00 |
| Instance_ValueTaskHandler          | True    | False |     9.785 ns |  0.0226 ns |  0.0200 ns |  0.62 |    0.01 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **558.737 ns** |  **5.5282 ns** |  **5.1711 ns** |  **1.00** |    **0.01** | **0.0143** |     **254 B** |        **1.00** |
| TaskSource_TaskHandler             | True    | True  |   576.082 ns |  5.9160 ns |  5.5339 ns |  1.03 |    0.01 | 0.0191 |     319 B |        1.26 |
| ValueTaskSource_TaskHandler        | True    | True  |   608.775 ns |  4.6480 ns |  4.1203 ns |  1.09 |    0.01 | 0.0238 |     405 B |        1.59 |
| Instance_TaskHandler               | True    | True  |    14.534 ns |  0.2010 ns |  0.1781 ns |  0.03 |    0.00 | 0.0048 |      80 B |        0.31 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   593.484 ns |  4.8519 ns |  4.3011 ns |  1.06 |    0.01 | 0.0181 |     308 B |        1.21 |
| Instance_ValueTaskHandler          | True    | True  |     9.446 ns |  0.0809 ns |  0.0756 ns |  0.02 |    0.00 |      - |         - |        0.00 |
