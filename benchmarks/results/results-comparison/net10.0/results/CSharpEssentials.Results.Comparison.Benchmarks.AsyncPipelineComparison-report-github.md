```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **106.24 ns** |  **0.966 ns** |  **0.904 ns** |  **1.00** |    **0.01** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   103.39 ns |  0.814 ns |  0.762 ns |  0.97 |    0.01 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   129.79 ns |  1.434 ns |  1.341 ns |  1.22 |    0.02 | 0.0386 |     648 B |        1.21 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,614.62 ns** |  **6.194 ns** |  **5.794 ns** |  **1.00** |    **0.00** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,588.20 ns |  8.723 ns |  6.810 ns |  0.98 |    0.01 | 0.0496 |     880 B |        0.92 |
| ErrorOr                    | False   | True  | 1,869.76 ns | 16.102 ns | 15.061 ns |  1.16 |    0.01 | 0.0648 |    1127 B |        1.17 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **89.92 ns** |  **0.652 ns** |  **0.610 ns** |  **1.00** |    **0.01** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   101.11 ns |  0.730 ns |  0.648 ns |  1.12 |    0.01 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   107.73 ns |  0.589 ns |  0.551 ns |  1.20 |    0.01 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,607.53 ns** |  **9.424 ns** |  **8.815 ns** |  **1.00** |    **0.01** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,636.71 ns |  9.568 ns |  8.950 ns |  1.02 |    0.01 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,872.45 ns | 12.069 ns | 10.698 ns |  1.16 |    0.01 | 0.0610 |    1037 B |        1.15 |
