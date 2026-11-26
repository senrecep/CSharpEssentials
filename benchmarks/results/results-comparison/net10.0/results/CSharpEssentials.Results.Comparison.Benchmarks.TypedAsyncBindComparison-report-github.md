```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.15 (10.0.1526.53008), X64 RyuJIT AVX2


```
| Method                            | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Task**       | **False**   | **False** |    **51.44 ns** |  **0.882 ns** |  **0.825 ns** |  **1.00** |    **0.02** | **0.0095** |     **160 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | False |    69.23 ns |  0.057 ns |  0.051 ns |  1.35 |    0.02 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | False   | False |    15.99 ns |  0.124 ns |  0.116 ns |  0.31 |    0.01 | 0.0048 |      80 B |        0.50 |
| CSharpEssentials_Result_ValueTask | False   | False |    34.12 ns |  0.107 ns |  0.100 ns |  0.66 |    0.01 |      - |         - |        0.00 |
| ErrorOr_Task                      | False   | False |    33.43 ns |  0.560 ns |  0.524 ns |  0.65 |    0.01 | 0.0095 |     160 B |        1.00 |
| CSharpFunctionalExtensions_Task   | False   | False |    33.66 ns |  0.168 ns |  0.149 ns |  0.65 |    0.01 | 0.0095 |     160 B |        1.00 |
| FluentResults_Task                | False   | False |    86.22 ns |  0.503 ns |  0.470 ns |  1.68 |    0.03 | 0.0210 |     352 B |        2.20 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **False**   | **True**  |   **853.58 ns** |  **7.027 ns** |  **6.573 ns** |  **1.00** |    **0.01** | **0.0210** |     **352 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | True  |   883.84 ns | 11.055 ns | 10.341 ns |  1.04 |    0.01 | 0.0210 |     359 B |        1.02 |
| CSharpEssentials_Result_Task      | False   | True  |   827.99 ns |  5.888 ns |  5.507 ns |  0.97 |    0.01 | 0.0153 |     263 B |        0.75 |
| CSharpEssentials_Result_ValueTask | False   | True  |   834.98 ns |  5.133 ns |  4.801 ns |  0.98 |    0.01 | 0.0210 |     358 B |        1.02 |
| ErrorOr_Task                      | False   | True  |   840.72 ns |  9.126 ns |  8.537 ns |  0.98 |    0.01 | 0.0200 |     343 B |        0.97 |
| CSharpFunctionalExtensions_Task   | False   | True  |   847.11 ns |  5.645 ns |  5.280 ns |  0.99 |    0.01 | 0.0200 |     344 B |        0.98 |
| FluentResults_Task                | False   | True  |   875.24 ns |  9.029 ns |  8.004 ns |  1.03 |    0.01 | 0.0296 |     504 B |        1.43 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **False** |    **56.78 ns** |  **0.300 ns** |  **0.281 ns** |  **1.00** |    **0.01** | **0.0143** |     **240 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | False |    71.69 ns |  0.081 ns |  0.076 ns |  1.26 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | True    | False |    21.91 ns |  0.120 ns |  0.101 ns |  0.39 |    0.00 | 0.0095 |     160 B |        0.67 |
| CSharpEssentials_Result_ValueTask | True    | False |    39.12 ns |  0.064 ns |  0.060 ns |  0.69 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | True    | False |    39.41 ns |  0.237 ns |  0.222 ns |  0.69 |    0.01 | 0.0143 |     240 B |        1.00 |
| CSharpFunctionalExtensions_Task   | True    | False |    30.64 ns |  0.170 ns |  0.151 ns |  0.54 |    0.00 | 0.0095 |     160 B |        0.67 |
| FluentResults_Task                | True    | False |   117.48 ns |  0.933 ns |  0.827 ns |  2.07 |    0.02 | 0.0319 |     536 B |        2.23 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **True**  | **1,622.01 ns** |  **9.199 ns** |  **8.604 ns** |  **1.00** |    **0.01** | **0.0305** |     **527 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | True  | 1,795.35 ns | 14.568 ns | 13.627 ns |  1.11 |    0.01 | 0.0401 |     662 B |        1.26 |
| CSharpEssentials_Result_Task      | True    | True  | 1,361.94 ns |  5.733 ns |  5.363 ns |  0.84 |    0.01 | 0.0229 |     384 B |        0.73 |
| CSharpEssentials_Result_ValueTask | True    | True  | 1,501.36 ns | 10.187 ns |  9.529 ns |  0.93 |    0.01 | 0.0286 |     496 B |        0.94 |
| ErrorOr_Task                      | True    | True  | 1,587.75 ns |  6.544 ns |  5.801 ns |  0.98 |    0.01 | 0.0305 |     518 B |        0.98 |
| CSharpFunctionalExtensions_Task   | True    | True  | 1,438.51 ns |  8.144 ns |  7.618 ns |  0.89 |    0.01 | 0.0229 |     408 B |        0.77 |
| FluentResults_Task                | True    | True  | 1,754.33 ns | 12.692 ns | 11.873 ns |  1.08 |    0.01 | 0.0458 |     768 B |        1.46 |
