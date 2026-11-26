```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                            | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Task**       | **False**   | **False** |    **98.85 ns** |  **0.211 ns** |  **0.197 ns** |  **1.00** |    **0.00** | **0.0095** |     **160 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | False |   152.05 ns |  0.082 ns |  0.077 ns |  1.54 |    0.00 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | False   | False |    21.67 ns |  0.066 ns |  0.062 ns |  0.22 |    0.00 | 0.0048 |      80 B |        0.50 |
| CSharpEssentials_Result_ValueTask | False   | False |    65.02 ns |  0.063 ns |  0.056 ns |  0.66 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | False   | False |    40.04 ns |  0.083 ns |  0.070 ns |  0.41 |    0.00 | 0.0095 |     160 B |        1.00 |
| CSharpFunctionalExtensions_Task   | False   | False |    74.86 ns |  0.435 ns |  0.407 ns |  0.76 |    0.00 | 0.0095 |     160 B |        1.00 |
| FluentResults_Task                | False   | False |   118.38 ns |  0.675 ns |  0.632 ns |  1.20 |    0.01 | 0.0215 |     360 B |        2.25 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **False**   | **True**  |   **894.24 ns** |  **7.504 ns** |  **7.019 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | True  | 1,023.57 ns | 20.161 ns | 22.409 ns |  1.14 |    0.03 | 0.0210 |     360 B |        1.02 |
| CSharpEssentials_Result_Task      | False   | True  |   825.66 ns |  4.039 ns |  3.581 ns |  0.92 |    0.01 | 0.0153 |     263 B |        0.75 |
| CSharpEssentials_Result_ValueTask | False   | True  |   886.79 ns |  4.656 ns |  4.355 ns |  0.99 |    0.01 | 0.0210 |     359 B |        1.02 |
| ErrorOr_Task                      | False   | True  |   836.84 ns |  5.756 ns |  5.384 ns |  0.94 |    0.01 | 0.0200 |     343 B |        0.97 |
| CSharpFunctionalExtensions_Task   | False   | True  |   885.12 ns |  7.603 ns |  7.112 ns |  0.99 |    0.01 | 0.0200 |     344 B |        0.98 |
| FluentResults_Task                | False   | True  |   944.94 ns |  9.355 ns |  8.751 ns |  1.06 |    0.01 | 0.0305 |     512 B |        1.45 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **False** |   **117.06 ns** |  **0.714 ns** |  **0.558 ns** |  **1.00** |    **0.01** | **0.0143** |     **240 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | False |   163.18 ns |  0.164 ns |  0.153 ns |  1.39 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | True    | False |    28.88 ns |  0.132 ns |  0.124 ns |  0.25 |    0.00 | 0.0095 |     160 B |        0.67 |
| CSharpEssentials_Result_ValueTask | True    | False |    60.64 ns |  0.072 ns |  0.060 ns |  0.52 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | True    | False |    44.86 ns |  0.189 ns |  0.176 ns |  0.38 |    0.00 | 0.0143 |     240 B |        1.00 |
| CSharpFunctionalExtensions_Task   | True    | False |    59.83 ns |  0.185 ns |  0.144 ns |  0.51 |    0.00 | 0.0095 |     160 B |        0.67 |
| FluentResults_Task                | True    | False |   192.22 ns |  0.769 ns |  0.720 ns |  1.64 |    0.01 | 0.0339 |     568 B |        2.37 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **True**  | **1,736.70 ns** | **14.983 ns** | **14.015 ns** |  **1.00** |    **0.01** | **0.0305** |     **528 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | True  | 1,944.16 ns | 36.649 ns | 37.636 ns |  1.12 |    0.02 | 0.0381 |     663 B |        1.26 |
| CSharpEssentials_Result_Task      | True    | True  | 1,407.33 ns |  6.905 ns |  6.459 ns |  0.81 |    0.01 | 0.0229 |     384 B |        0.73 |
| CSharpEssentials_Result_ValueTask | True    | True  | 1,541.59 ns | 13.300 ns | 12.441 ns |  0.89 |    0.01 | 0.0267 |     496 B |        0.94 |
| ErrorOr_Task                      | True    | True  | 1,604.54 ns |  7.319 ns |  6.847 ns |  0.92 |    0.01 | 0.0305 |     518 B |        0.98 |
| CSharpFunctionalExtensions_Task   | True    | True  | 1,495.60 ns |  5.315 ns |  4.971 ns |  0.86 |    0.01 | 0.0229 |     384 B |        0.73 |
| FluentResults_Task                | True    | True  | 1,862.54 ns | 36.377 ns | 35.728 ns |  1.07 |    0.02 | 0.0458 |     800 B |        1.52 |
