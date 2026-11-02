```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **123.45 ns** |  **1.815 ns** |  **1.698 ns** |  **1.00** |    **0.02** | **0.0348** |     **584 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   109.73 ns |  1.402 ns |  1.312 ns |  0.89 |    0.02 | 0.0286 |     480 B |        0.82 |
| ErrorOr                    | False   | False |   132.43 ns |  0.920 ns |  0.768 ns |  1.07 |    0.02 | 0.0386 |     648 B |        1.11 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,680.14 ns** |  **9.914 ns** |  **8.789 ns** |  **1.00** |    **0.01** | **0.0610** |    **1048 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,625.30 ns | 18.112 ns | 16.056 ns |  0.97 |    0.01 | 0.0515 |     879 B |        0.84 |
| ErrorOr                    | False   | True  | 1,926.91 ns | 27.869 ns | 23.272 ns |  1.15 |    0.01 | 0.0648 |    1126 B |        1.07 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **95.07 ns** |  **1.705 ns** |  **1.512 ns** |  **1.00** |    **0.02** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   104.30 ns |  0.766 ns |  0.717 ns |  1.10 |    0.02 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   112.01 ns |  1.437 ns |  1.344 ns |  1.18 |    0.02 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,591.97 ns** | **17.325 ns** | **16.205 ns** |  **1.00** |    **0.01** | **0.0534** |     **943 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,609.77 ns | 13.222 ns | 12.368 ns |  1.01 |    0.01 | 0.0496 |     879 B |        0.93 |
| ErrorOr                    | True    | True  | 1,849.47 ns | 16.051 ns | 13.403 ns |  1.16 |    0.01 | 0.0610 |    1036 B |        1.10 |
