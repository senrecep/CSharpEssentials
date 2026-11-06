```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                                | Categories | N   | Succeed | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |----------- |---- |-------- |-------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **False**   |    **34.209 ns** |  **0.0924 ns** |  **0.0771 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | False   |   226.437 ns |  1.0265 ns |  0.9602 ns |  6.62 |    0.03 | 0.0319 |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 10  | False   |    34.382 ns |  0.2883 ns |  0.2696 ns |  1.01 |    0.01 | 0.0110 |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 10  | False   |   219.359 ns |  0.4326 ns |  0.3835 ns |  6.41 |    0.02 | 0.0148 |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **10**  | **True**    |     **8.953 ns** |  **0.0355 ns** |  **0.0315 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 10  | True    |    30.793 ns |  0.1806 ns |  0.1689 ns |  3.44 |    0.02 | 0.0052 |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 10  | True    |    11.721 ns |  0.0413 ns |  0.0386 ns |  1.31 |    0.01 | 0.0019 |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 10  | True    |   164.302 ns |  2.0699 ns |  1.9362 ns | 18.35 |    0.22 | 0.0091 |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **False**   |   **145.116 ns** |  **0.1605 ns** |  **0.1253 ns** |  **1.00** |    **0.00** | **0.0033** |      **56 B** |        **1.00** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | False   |   279.705 ns |  1.6259 ns |  1.5209 ns |  1.93 |    0.01 | 0.0319 |     536 B |        9.57 |
| ErrorOr_CombineAll                    | CombineAll | 100 | False   |    67.431 ns |  0.4335 ns |  0.4055 ns |  0.46 |    0.00 | 0.0110 |     184 B |        3.29 |
| FluentResults_CombineAll              | CombineAll | 100 | False   | 1,199.267 ns |  1.2333 ns |  1.0298 ns |  8.26 |    0.01 | 0.0134 |     248 B |        4.43 |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_CombineAll**           | **CombineAll** | **100** | **True**    |    **68.676 ns** |  **0.3453 ns** |  **0.3061 ns** |  **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions_CombineAll | CombineAll | 100 | True    |    84.664 ns |  0.6095 ns |  0.5403 ns |  1.23 |    0.01 | 0.0052 |      88 B |          NA |
| ErrorOr_CombineAll                    | CombineAll | 100 | True    |    43.120 ns |  0.0739 ns |  0.0655 ns |  0.63 |    0.00 | 0.0019 |      32 B |          NA |
| FluentResults_CombineAll              | CombineAll | 100 | True    | 1,095.284 ns |  2.5333 ns |  2.2457 ns | 15.95 |    0.08 | 0.0076 |     152 B |          NA |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **False**   |   **123.726 ns** |  **1.1791 ns** |  **1.1029 ns** |  **1.00** |    **0.01** | **0.0291** |     **488 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | False   |   371.742 ns |  2.6445 ns |  2.4737 ns |  3.00 |    0.03 | 0.0505 |     848 B |        1.74 |
| ErrorOr_Sequence                      | Sequence   | 10  | False   |    92.114 ns |  1.0731 ns |  1.0038 ns |  0.74 |    0.01 | 0.0238 |     400 B |        0.82 |
| FluentResults_Sequence                | Sequence   | 10  | False   |   368.976 ns |  1.3695 ns |  1.2810 ns |  2.98 |    0.03 | 0.0339 |     568 B |        1.16 |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **10**  | **True**    |    **93.941 ns** |  **0.7493 ns** |  **0.7009 ns** |  **1.00** |    **0.01** | **0.0205** |     **344 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 10  | True    |   335.289 ns |  1.0091 ns |  0.9439 ns |  3.57 |    0.03 | 0.0324 |     544 B |        1.58 |
| ErrorOr_Sequence                      | Sequence   | 10  | True    |    63.012 ns |  0.4692 ns |  0.4389 ns |  0.67 |    0.01 | 0.0148 |     248 B |        0.72 |
| FluentResults_Sequence                | Sequence   | 10  | True    |   596.584 ns |  2.4870 ns |  2.3263 ns |  6.35 |    0.05 | 0.0668 |    1128 B |        3.28 |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **False**   |   **491.948 ns** |  **2.7050 ns** |  **2.5303 ns** |  **1.00** |    **0.01** | **0.0868** |    **1456 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | False   | 1,006.790 ns | 11.6391 ns | 10.8873 ns |  2.05 |    0.02 | 0.1354 |    2288 B |        1.57 |
| ErrorOr_Sequence                      | Sequence   | 100 | False   |   341.549 ns |  3.5932 ns |  3.3611 ns |  0.69 |    0.01 | 0.0815 |    1368 B |        0.94 |
| FluentResults_Sequence                | Sequence   | 100 | False   | 1,574.568 ns |  3.0711 ns |  2.8727 ns |  3.20 |    0.02 | 0.0763 |    1288 B |        0.88 |
|                                       |            |     |         |              |            |            |       |         |        |           |             |
| **CSharpEssentials_Sequence**             | **Sequence**   | **100** | **True**    |   **475.552 ns** |  **3.6821 ns** |  **3.2641 ns** |  **1.00** |    **0.01** | **0.0992** |    **1672 B** |        **1.00** |
| CSharpFunctionalExtensions_Sequence   | Sequence   | 100 | True    | 2,303.390 ns |  9.5058 ns |  8.8917 ns |  4.84 |    0.04 | 0.1373 |    2344 B |        1.40 |
| ErrorOr_Sequence                      | Sequence   | 100 | True    |   239.739 ns |  1.5910 ns |  1.4882 ns |  0.50 |    0.00 | 0.0725 |    1216 B |        0.73 |
| FluentResults_Sequence                | Sequence   | 100 | True    | 3,786.225 ns |  8.0186 ns |  6.6959 ns |  7.96 |    0.05 | 0.3891 |    6528 B |        3.90 |
