```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **113.8 ns** |  **0.55 ns** |  **0.46 ns** |  **1.00** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   109.1 ns |  0.60 ns |  0.56 ns |  0.96 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   143.5 ns |  1.54 ns |  1.44 ns |  1.26 | 0.0386 |     648 B |        1.21 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,582.9 ns** |  **8.93 ns** |  **8.35 ns** |  **1.00** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,550.8 ns |  5.12 ns |  4.79 ns |  0.98 | 0.0515 |     879 B |        0.92 |
| ErrorOr                    | False   | True  | 1,885.9 ns | 11.44 ns | 10.71 ns |  1.19 | 0.0668 |    1126 B |        1.17 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |   **109.6 ns** |  **0.83 ns** |  **0.77 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   113.0 ns |  0.80 ns |  0.75 ns |  1.03 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   118.2 ns |  0.61 ns |  0.54 ns |  1.08 | 0.0334 |     560 B |        1.17 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,572.1 ns** |  **9.03 ns** |  **8.01 ns** |  **1.00** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,611.5 ns | 12.89 ns | 11.43 ns |  1.03 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,865.3 ns | 12.47 ns | 11.06 ns |  1.19 | 0.0610 |    1037 B |        1.15 |
