```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **106.41 ns** |  **0.426 ns** |  **0.399 ns** |  **1.00** | **0.0348** |     **584 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |    95.71 ns |  0.478 ns |  0.447 ns |  0.90 | 0.0286 |     480 B |        0.82 |
| ErrorOr                    | False   | False |   111.24 ns |  0.882 ns |  0.688 ns |  1.05 | 0.0386 |     648 B |        1.11 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **2,967.87 ns** | **26.899 ns** | **25.161 ns** |  **1.00** | **0.0610** |    **1048 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 2,934.78 ns | 24.989 ns | 22.152 ns |  0.99 | 0.0496 |     880 B |        0.84 |
| ErrorOr                    | False   | True  | 2,919.01 ns | 34.601 ns | 32.366 ns |  0.98 | 0.0648 |    1128 B |        1.08 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **84.53 ns** |  **0.561 ns** |  **0.525 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |    94.51 ns |  0.972 ns |  0.909 ns |  1.12 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |    99.78 ns |  0.553 ns |  0.490 ns |  1.18 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **3,009.78 ns** | **25.898 ns** | **24.225 ns** |  **1.00** | **0.0534** |     **944 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 2,993.30 ns | 26.926 ns | 25.186 ns |  0.99 | 0.0458 |     880 B |        0.93 |
| ErrorOr                    | True    | True  | 2,876.75 ns | 20.268 ns | 17.967 ns |  0.96 | 0.0610 |    1039 B |        1.10 |
