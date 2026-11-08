```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **123.6 ns** |  **1.04 ns** |  **0.97 ns** |  **1.00** |    **0.01** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   113.7 ns |  0.71 ns |  0.66 ns |  0.92 |    0.01 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   162.7 ns |  0.95 ns |  0.89 ns |  1.32 |    0.01 | 0.0386 |     648 B |        1.21 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,648.5 ns** | **13.43 ns** | **12.56 ns** |  **1.00** |    **0.01** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,591.0 ns |  9.63 ns |  8.54 ns |  0.97 |    0.01 | 0.0515 |     880 B |        0.92 |
| ErrorOr                    | False   | True  | 1,902.4 ns | 15.15 ns | 14.17 ns |  1.15 |    0.01 | 0.0668 |    1127 B |        1.17 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |   **103.9 ns** |  **0.88 ns** |  **0.82 ns** |  **1.00** |    **0.01** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   115.4 ns |  0.78 ns |  0.61 ns |  1.11 |    0.01 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   122.0 ns |  1.40 ns |  1.31 ns |  1.17 |    0.02 | 0.0334 |     560 B |        1.17 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,628.6 ns** | **14.27 ns** | **13.35 ns** |  **1.00** |    **0.01** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,656.9 ns | 13.04 ns | 11.56 ns |  1.02 |    0.01 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,884.8 ns | 16.03 ns | 13.39 ns |  1.16 |    0.01 | 0.0610 |    1038 B |        1.15 |
