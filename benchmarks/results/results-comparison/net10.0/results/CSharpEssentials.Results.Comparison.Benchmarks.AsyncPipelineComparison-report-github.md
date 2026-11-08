```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.14 (10.0.1426.52326), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **103.31 ns** |  **0.606 ns** |  **0.567 ns** |  **1.00** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |    98.65 ns |  0.649 ns |  0.542 ns |  0.95 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   123.15 ns |  1.185 ns |  1.109 ns |  1.19 | 0.0386 |     648 B |        1.21 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,586.32 ns** | **11.826 ns** | **11.062 ns** |  **1.00** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,558.09 ns | 12.397 ns | 11.596 ns |  0.98 | 0.0515 |     879 B |        0.92 |
| ErrorOr                    | False   | True  | 1,848.80 ns | 10.486 ns |  8.757 ns |  1.17 | 0.0668 |    1127 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **84.00 ns** |  **0.498 ns** |  **0.442 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |    95.84 ns |  0.784 ns |  0.695 ns |  1.14 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   103.95 ns |  0.956 ns |  0.848 ns |  1.24 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,580.19 ns** | **12.809 ns** | **11.355 ns** |  **1.00** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,594.53 ns | 10.786 ns | 11.988 ns |  1.01 | 0.0496 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,836.72 ns | 13.044 ns | 12.202 ns |  1.16 | 0.0610 |    1037 B |        1.15 |
