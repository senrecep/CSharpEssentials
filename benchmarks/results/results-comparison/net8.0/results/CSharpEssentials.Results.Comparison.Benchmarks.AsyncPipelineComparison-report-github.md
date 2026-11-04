```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **137.9 ns** |  **1.82 ns** |  **1.52 ns** |  **1.00** |    **0.01** | **0.0348** |     **584 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   112.7 ns |  0.73 ns |  0.68 ns |  0.82 |    0.01 | 0.0286 |     480 B |        0.82 |
| ErrorOr                    | False   | False |   142.7 ns |  1.80 ns |  1.69 ns |  1.03 |    0.02 | 0.0386 |     648 B |        1.11 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,639.0 ns** | **14.57 ns** | **13.62 ns** |  **1.00** |    **0.01** | **0.0610** |    **1048 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,582.6 ns | 17.05 ns | 15.95 ns |  0.97 |    0.01 | 0.0515 |     879 B |        0.84 |
| ErrorOr                    | False   | True  | 1,871.5 ns | 16.25 ns | 14.40 ns |  1.14 |    0.01 | 0.0668 |    1127 B |        1.08 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |   **104.3 ns** |  **0.89 ns** |  **0.83 ns** |  **1.00** |    **0.01** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   115.1 ns |  0.51 ns |  0.47 ns |  1.10 |    0.01 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   123.3 ns |  1.16 ns |  1.03 ns |  1.18 |    0.01 | 0.0334 |     560 B |        1.17 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,630.7 ns** | **10.65 ns** |  **9.96 ns** |  **1.00** |    **0.01** | **0.0553** |     **944 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,656.9 ns | 14.24 ns | 12.63 ns |  1.02 |    0.01 | 0.0515 |     879 B |        0.93 |
| ErrorOr                    | True    | True  | 1,850.1 ns | 25.32 ns | 23.68 ns |  1.13 |    0.02 | 0.0610 |    1037 B |        1.10 |
