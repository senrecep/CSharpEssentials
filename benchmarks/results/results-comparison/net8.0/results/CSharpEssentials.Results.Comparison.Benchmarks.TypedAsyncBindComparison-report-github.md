```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                            | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Task**       | **False**   | **False** |    **47.93 ns** |  **0.158 ns** |  **0.148 ns** |  **1.00** |    **0.00** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | False |    88.86 ns |  0.151 ns |  0.134 ns |  1.85 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | False   | False |    21.42 ns |  0.166 ns |  0.147 ns |  0.45 |    0.00 | 0.0048 |      80 B |        1.00 |
| CSharpEssentials_Result_ValueTask | False   | False |    63.20 ns |  0.045 ns |  0.042 ns |  1.32 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | False   | False |    39.70 ns |  0.180 ns |  0.169 ns |  0.83 |    0.00 | 0.0095 |     160 B |        2.00 |
| CSharpFunctionalExtensions_Task   | False   | False |    76.32 ns |  0.232 ns |  0.193 ns |  1.59 |    0.01 | 0.0095 |     160 B |        2.00 |
| FluentResults_Task                | False   | False |   125.14 ns |  0.626 ns |  0.585 ns |  2.61 |    0.01 | 0.0215 |     360 B |        4.50 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **False**   | **True**  |   **867.62 ns** |  **5.866 ns** |  **5.487 ns** |  **1.00** |    **0.01** | **0.0162** |     **272 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | True  |   970.91 ns | 19.262 ns | 25.714 ns |  1.12 |    0.03 | 0.0210 |     360 B |        1.32 |
| CSharpEssentials_Result_Task      | False   | True  |   831.33 ns |  4.693 ns |  4.390 ns |  0.96 |    0.01 | 0.0153 |     263 B |        0.97 |
| CSharpEssentials_Result_ValueTask | False   | True  |   904.50 ns |  3.023 ns |  2.680 ns |  1.04 |    0.01 | 0.0210 |     358 B |        1.32 |
| ErrorOr_Task                      | False   | True  |   857.88 ns |  6.376 ns |  5.964 ns |  0.99 |    0.01 | 0.0200 |     343 B |        1.26 |
| CSharpFunctionalExtensions_Task   | False   | True  |   894.48 ns |  8.545 ns |  7.575 ns |  1.03 |    0.01 | 0.0200 |     344 B |        1.26 |
| FluentResults_Task                | False   | True  |   951.37 ns |  7.947 ns |  7.433 ns |  1.10 |    0.01 | 0.0305 |     512 B |        1.88 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **False** |    **61.78 ns** |  **0.181 ns** |  **0.160 ns** |  **1.00** |    **0.00** | **0.0095** |     **160 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | False |   118.90 ns |  0.287 ns |  0.240 ns |  1.92 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | True    | False |    28.41 ns |  0.230 ns |  0.216 ns |  0.46 |    0.00 | 0.0095 |     160 B |        1.00 |
| CSharpEssentials_Result_ValueTask | True    | False |    58.24 ns |  0.087 ns |  0.082 ns |  0.94 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | True    | False |    44.89 ns |  0.775 ns |  0.687 ns |  0.73 |    0.01 | 0.0143 |     240 B |        1.50 |
| CSharpFunctionalExtensions_Task   | True    | False |    60.85 ns |  0.219 ns |  0.205 ns |  0.98 |    0.00 | 0.0095 |     160 B |        1.00 |
| FluentResults_Task                | True    | False |   198.12 ns |  0.936 ns |  0.876 ns |  3.21 |    0.02 | 0.0339 |     568 B |        3.55 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **True**  | **1,525.08 ns** |  **8.672 ns** |  **8.112 ns** |  **1.00** |    **0.01** | **0.0229** |     **392 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | True  | 1,771.95 ns | 21.877 ns | 20.464 ns |  1.16 |    0.01 | 0.0286 |     496 B |        1.27 |
| CSharpEssentials_Result_Task      | True    | True  | 1,407.75 ns |  6.795 ns |  6.356 ns |  0.92 |    0.01 | 0.0229 |     384 B |        0.98 |
| CSharpEssentials_Result_ValueTask | True    | True  | 1,552.89 ns | 11.192 ns | 10.469 ns |  1.02 |    0.01 | 0.0286 |     496 B |        1.27 |
| ErrorOr_Task                      | True    | True  | 1,576.16 ns |  6.537 ns |  5.795 ns |  1.03 |    0.01 | 0.0305 |     518 B |        1.32 |
| CSharpFunctionalExtensions_Task   | True    | True  | 1,514.95 ns |  6.727 ns |  5.963 ns |  0.99 |    0.01 | 0.0229 |     384 B |        0.98 |
| FluentResults_Task                | True    | True  | 1,849.64 ns | 19.091 ns | 17.858 ns |  1.21 |    0.01 | 0.0477 |     800 B |        2.04 |
