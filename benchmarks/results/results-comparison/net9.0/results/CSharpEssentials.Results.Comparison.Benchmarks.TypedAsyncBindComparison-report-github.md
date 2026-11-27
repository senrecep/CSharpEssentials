```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                            | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed_Task**       | **False**   | **False** |    **24.63 ns** |  **0.313 ns** |  **0.262 ns** |  **1.00** |    **0.01** | **0.0048** |      **80 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | False |    62.38 ns |  0.128 ns |  0.119 ns |  2.53 |    0.03 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | False   | False |    19.05 ns |  0.087 ns |  0.077 ns |  0.77 |    0.01 | 0.0048 |      80 B |        1.00 |
| CSharpEssentials_Result_ValueTask | False   | False |    52.64 ns |  0.075 ns |  0.063 ns |  2.14 |    0.02 |      - |         - |        0.00 |
| ErrorOr_Task                      | False   | False |    36.42 ns |  0.247 ns |  0.231 ns |  1.48 |    0.02 | 0.0095 |     160 B |        2.00 |
| CSharpFunctionalExtensions_Task   | False   | False |    37.15 ns |  0.299 ns |  0.280 ns |  1.51 |    0.02 | 0.0095 |     160 B |        2.00 |
| FluentResults_Task                | False   | False |   108.73 ns |  0.491 ns |  0.459 ns |  4.42 |    0.05 | 0.0210 |     352 B |        4.40 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **False**   | **True**  |   **847.52 ns** |  **5.076 ns** |  **4.239 ns** |  **1.00** |    **0.01** | **0.0162** |     **271 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | False   | True  |   893.29 ns |  4.866 ns |  4.551 ns |  1.05 |    0.01 | 0.0210 |     359 B |        1.32 |
| CSharpEssentials_Result_Task      | False   | True  |   837.77 ns |  5.374 ns |  5.027 ns |  0.99 |    0.01 | 0.0153 |     263 B |        0.97 |
| CSharpEssentials_Result_ValueTask | False   | True  |   872.52 ns |  3.935 ns |  3.681 ns |  1.03 |    0.01 | 0.0210 |     358 B |        1.32 |
| ErrorOr_Task                      | False   | True  |   855.66 ns |  6.074 ns |  5.681 ns |  1.01 |    0.01 | 0.0200 |     343 B |        1.27 |
| CSharpFunctionalExtensions_Task   | False   | True  |   873.50 ns |  4.529 ns |  4.236 ns |  1.03 |    0.01 | 0.0200 |     344 B |        1.27 |
| FluentResults_Task                | False   | True  |   918.81 ns |  6.535 ns |  5.793 ns |  1.08 |    0.01 | 0.0296 |     504 B |        1.86 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **False** |    **37.39 ns** |  **0.133 ns** |  **0.118 ns** |  **1.00** |    **0.00** | **0.0095** |     **160 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | False |    88.64 ns |  0.067 ns |  0.059 ns |  2.37 |    0.01 |      - |         - |        0.00 |
| CSharpEssentials_Result_Task      | True    | False |    24.98 ns |  0.135 ns |  0.120 ns |  0.67 |    0.00 | 0.0095 |     160 B |        1.00 |
| CSharpEssentials_Result_ValueTask | True    | False |    58.43 ns |  0.039 ns |  0.036 ns |  1.56 |    0.00 |      - |         - |        0.00 |
| ErrorOr_Task                      | True    | False |    43.28 ns |  0.167 ns |  0.148 ns |  1.16 |    0.01 | 0.0143 |     240 B |        1.50 |
| CSharpFunctionalExtensions_Task   | True    | False |    38.23 ns |  0.197 ns |  0.184 ns |  1.02 |    0.01 | 0.0095 |     160 B |        1.00 |
| FluentResults_Task                | True    | False |   163.80 ns |  0.563 ns |  0.440 ns |  4.38 |    0.02 | 0.0319 |     536 B |        3.35 |
|                                   |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed_Task**       | **True**    | **True**  | **1,431.80 ns** |  **4.171 ns** |  **3.901 ns** |  **1.00** |    **0.00** | **0.0229** |     **392 B** |        **1.00** |
| CSharpEssentials_Typed_ValueTask  | True    | True  | 1,602.42 ns |  9.862 ns |  9.225 ns |  1.12 |    0.01 | 0.0286 |     496 B |        1.27 |
| CSharpEssentials_Result_Task      | True    | True  | 1,361.92 ns |  6.582 ns |  5.835 ns |  0.95 |    0.00 | 0.0229 |     384 B |        0.98 |
| CSharpEssentials_Result_ValueTask | True    | True  | 1,511.01 ns |  7.052 ns |  6.597 ns |  1.06 |    0.01 | 0.0286 |     496 B |        1.27 |
| ErrorOr_Task                      | True    | True  | 1,595.89 ns |  8.909 ns |  8.334 ns |  1.11 |    0.01 | 0.0305 |     518 B |        1.32 |
| CSharpFunctionalExtensions_Task   | True    | True  | 1,439.27 ns |  6.393 ns |  5.980 ns |  1.01 |    0.00 | 0.0229 |     408 B |        1.04 |
| FluentResults_Task                | True    | True  | 1,778.27 ns | 13.021 ns | 12.180 ns |  1.24 |    0.01 | 0.0458 |     768 B |        1.96 |
