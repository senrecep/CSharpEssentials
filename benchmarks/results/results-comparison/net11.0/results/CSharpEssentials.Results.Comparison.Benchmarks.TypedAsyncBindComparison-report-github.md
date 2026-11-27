```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                            | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Task**       | **False**   | **False** |    **22.12 ns** |  **0.160 ns** |  **0.142 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | False |    30.50 ns |  0.074 ns |  0.066 ns |  1.38 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | False   | False |    15.11 ns |  0.120 ns |  0.112 ns |  0.68 |    0.01 | 0.0048 |      80 B |        1.00 |
| CSharpEssentials_Result_ValueTask | False   | False |    31.97 ns |  0.093 ns |  0.087 ns |  1.45 |    0.01 |      - |         - |        0.00 |
| ErrorOr_Task                      | False   | False |    30.71 ns |  0.150 ns |  0.141 ns |  1.39 |    0.01 | 0.0095 |     160 B |        2.00 |
| CSharpFunctionalExtensions_Task   | False   | False |    32.73 ns |  0.268 ns |  0.251 ns |  1.48 |    0.01 | 0.0095 |     160 B |        2.00 |
| FluentResults_Task                | False   | False |    82.04 ns |  0.705 ns |  0.659 ns |  3.71 |    0.04 | 0.0210 |     352 B |        4.40 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **False**   | **True**  |   **586.00 ns** |  **2.551 ns** |  **2.130 ns** |  **1.00** |    **0.00** | **0.0153** |     **271 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | True  |   654.13 ns | 10.256 ns |  9.594 ns |  1.12 |    0.02 | 0.0210 |     358 B |        1.32 |
| CSharpEssentials_Result_Task      | False   | True  |   565.71 ns |  4.208 ns |  3.936 ns |  0.97 |    0.01 | 0.0153 |     263 B |        0.97 |
| CSharpEssentials_Result_ValueTask | False   | True  |   622.93 ns |  5.738 ns |  5.367 ns |  1.06 |    0.01 | 0.0210 |     357 B |        1.32 |
| ErrorOr_Task                      | False   | True  |   605.07 ns |  8.526 ns |  7.976 ns |  1.03 |    0.01 | 0.0200 |     342 B |        1.26 |
| CSharpFunctionalExtensions_Task   | False   | True  |   598.85 ns | 10.842 ns | 10.142 ns |  1.02 |    0.02 | 0.0200 |     343 B |        1.27 |
| FluentResults_Task                | False   | True  |   690.86 ns |  5.523 ns |  4.896 ns |  1.18 |    0.01 | 0.0296 |     504 B |        1.86 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **False** |    **30.27 ns** |  **0.134 ns** |  **0.119 ns** |  **1.00** |    **0.01** | **0.0095** |     **160 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | False |    38.02 ns |  0.090 ns |  0.084 ns |  1.26 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | True    | False |    21.63 ns |  0.210 ns |  0.196 ns |  0.71 |    0.01 | 0.0095 |     160 B |        1.00 |
| CSharpEssentials_Result_ValueTask | True    | False |    40.79 ns |  0.040 ns |  0.038 ns |  1.35 |    0.01 |      - |         - |        0.00 |
| ErrorOr_Task                      | True    | False |    37.45 ns |  0.296 ns |  0.277 ns |  1.24 |    0.01 | 0.0143 |     240 B |        1.50 |
| CSharpFunctionalExtensions_Task   | True    | False |    29.82 ns |  0.223 ns |  0.209 ns |  0.99 |    0.01 | 0.0095 |     160 B |        1.00 |
| FluentResults_Task                | True    | False |   100.53 ns |  0.735 ns |  0.688 ns |  3.32 |    0.03 | 0.0319 |     536 B |        3.35 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **True**  |   **963.26 ns** |  **7.211 ns** |  **6.745 ns** |  **1.00** |    **0.01** | **0.0229** |     **392 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | True  | 1,050.38 ns | 11.119 ns |  9.856 ns |  1.09 |    0.01 | 0.0229 |     496 B |        1.27 |
| CSharpEssentials_Result_Task      | True    | True  |   971.74 ns | 12.958 ns | 12.121 ns |  1.01 |    0.01 | 0.0229 |     384 B |        0.98 |
| CSharpEssentials_Result_ValueTask | True    | True  | 1,082.19 ns | 11.173 ns | 10.451 ns |  1.12 |    0.01 | 0.0286 |     496 B |        1.27 |
| ErrorOr_Task                      | True    | True  | 1,057.86 ns |  7.597 ns |  6.734 ns |  1.10 |    0.01 | 0.0305 |     516 B |        1.32 |
| CSharpFunctionalExtensions_Task   | True    | True  |   983.00 ns |  6.166 ns |  5.466 ns |  1.02 |    0.01 | 0.0229 |     408 B |        1.04 |
| FluentResults_Task                | True    | True  | 1,247.21 ns |  9.893 ns |  8.770 ns |  1.29 |    0.01 | 0.0458 |     767 B |        1.96 |
