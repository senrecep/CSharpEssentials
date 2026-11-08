```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                             | Succeed | Yield | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|----------------------------------- |-------- |------ |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **HandWritten_TaskSource_TaskHandler** | **False**   | **False** |    **15.710 ns** |  **0.1855 ns** |  **0.1735 ns** |  **1.00** |    **0.02** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | False   | False |    15.431 ns |  0.1525 ns |  0.1427 ns |  0.98 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | False   | False |    23.096 ns |  0.2979 ns |  0.2787 ns |  1.47 |    0.02 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | False   | False |    18.394 ns |  0.0317 ns |  0.0281 ns |  1.17 |    0.01 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | False   | False |    19.138 ns |  0.0274 ns |  0.0229 ns |  1.22 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | False   | False |     8.048 ns |  0.0675 ns |  0.0631 ns |  0.51 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | False   | False |     2.829 ns |  0.0043 ns |  0.0038 ns |  0.18 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **False**   | **True**  |   **581.310 ns** |  **5.5911 ns** |  **5.2299 ns** | **1.000** |    **0.01** | **0.0143** |     **254 B** |        **1.00** |
| TaskSource_Action                  | False   | True  |   552.318 ns |  2.7416 ns |  2.4304 ns | 0.950 |    0.01 | 0.0134 |     231 B |        0.91 |
| TaskSource_TaskHandler             | False   | True  |   565.774 ns |  8.8653 ns |  8.2926 ns | 0.973 |    0.02 | 0.0191 |     319 B |        1.26 |
| ValueTaskSource_Action             | False   | True  |   615.653 ns |  3.7958 ns |  3.3649 ns | 1.059 |    0.01 | 0.0181 |     309 B |        1.22 |
| ValueTaskSource_ValueTaskHandler   | False   | True  |   613.004 ns |  5.5337 ns |  5.1763 ns | 1.055 |    0.01 | 0.0200 |     340 B |        1.34 |
| Instance_TaskHandler               | False   | True  |     8.159 ns |  0.1454 ns |  0.1428 ns | 0.014 |    0.00 | 0.0048 |      80 B |        0.31 |
| Instance_ValueTaskHandler          | False   | True  |     2.823 ns |  0.0125 ns |  0.0117 ns | 0.005 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **False** |    **18.410 ns** |  **0.1072 ns** |  **0.0896 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| TaskSource_Action                  | True    | False |    15.518 ns |  0.1742 ns |  0.1360 ns |  0.84 |    0.01 | 0.0048 |      80 B |        1.00 |
| TaskSource_TaskHandler             | True    | False |    34.011 ns |  0.2040 ns |  0.1908 ns |  1.85 |    0.01 | 0.0095 |     160 B |        2.00 |
| ValueTaskSource_Action             | True    | False |    15.885 ns |  0.0476 ns |  0.0445 ns |  0.86 |    0.00 |      - |         - |        0.00 |
| ValueTaskSource_ValueTaskHandler   | True    | False |    22.094 ns |  0.0191 ns |  0.0159 ns |  1.20 |    0.01 |      - |         - |        0.00 |
| Instance_TaskHandler               | True    | False |    16.273 ns |  0.1712 ns |  0.1518 ns |  0.88 |    0.01 | 0.0048 |      80 B |        1.00 |
| Instance_ValueTaskHandler          | True    | False |    12.784 ns |  0.0238 ns |  0.0199 ns |  0.69 |    0.00 |      - |         - |        0.00 |
|                                    |         |       |              |            |            |       |         |        |           |             |
| **HandWritten_TaskSource_TaskHandler** | **True**    | **True**  |   **923.997 ns** |  **5.7362 ns** |  **5.0850 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| TaskSource_Action                  | True    | True  |   584.241 ns |  7.5462 ns |  7.0587 ns |  0.63 |    0.01 | 0.0134 |     231 B |        0.66 |
| TaskSource_TaskHandler             | True    | True  | 1,050.760 ns | 12.5492 ns | 11.7385 ns |  1.14 |    0.01 | 0.0286 |     476 B |        1.35 |
| ValueTaskSource_Action             | True    | True  |   616.395 ns |  5.0428 ns |  4.7170 ns |  0.67 |    0.01 | 0.0181 |     309 B |        0.88 |
| ValueTaskSource_ValueTaskHandler   | True    | True  |   977.598 ns |  5.2498 ns |  4.6538 ns |  1.06 |    0.01 | 0.0248 |     440 B |        1.25 |
| Instance_TaskHandler               | True    | True  |   543.693 ns |  5.7270 ns |  5.0768 ns |  0.59 |    0.01 | 0.0134 |     239 B |        0.68 |
| Instance_ValueTaskHandler          | True    | True  |   597.675 ns | 11.6310 ns |  9.7124 ns |  0.65 |    0.01 | 0.0143 |     254 B |        0.72 |
