```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **129.01 ns** |  **1.457 ns** |  **1.291 ns** |  **1.00** | **0.0348** |     **584 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   107.45 ns |  0.435 ns |  0.407 ns |  0.83 | 0.0286 |     480 B |        0.82 |
| ErrorOr                    | False   | False |   136.02 ns |  0.712 ns |  0.631 ns |  1.05 | 0.0386 |     648 B |        1.11 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,602.28 ns** | **10.552 ns** |  **9.354 ns** |  **1.00** | **0.0610** |    **1048 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,539.33 ns |  7.742 ns |  6.863 ns |  0.96 | 0.0515 |     880 B |        0.84 |
| ErrorOr                    | False   | True  | 1,873.26 ns |  8.670 ns |  8.110 ns |  1.17 | 0.0668 |    1126 B |        1.07 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **99.60 ns** |  **0.684 ns** |  **0.639 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   111.02 ns |  0.867 ns |  0.811 ns |  1.11 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   117.73 ns |  1.252 ns |  1.171 ns |  1.18 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,599.93 ns** | **10.571 ns** |  **9.888 ns** |  **1.00** | **0.0553** |     **944 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,594.73 ns | 16.150 ns | 14.316 ns |  1.00 | 0.0515 |     880 B |        0.93 |
| ErrorOr                    | True    | True  | 1,866.50 ns | 16.185 ns | 15.140 ns |  1.17 | 0.0610 |    1037 B |        1.10 |
