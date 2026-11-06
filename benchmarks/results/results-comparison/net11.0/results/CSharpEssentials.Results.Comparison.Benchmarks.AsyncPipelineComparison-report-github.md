```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **101.35 ns** |  **1.048 ns** |  **0.981 ns** |  **1.00** |    **0.01** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   103.56 ns |  0.919 ns |  0.814 ns |  1.02 |    0.01 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   123.30 ns |  1.280 ns |  1.198 ns |  1.22 |    0.02 | 0.0386 |     648 B |        1.21 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **3,128.82 ns** | **53.351 ns** | **49.905 ns** |  **1.00** |    **0.02** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 3,080.85 ns | 35.900 ns | 33.580 ns |  0.98 |    0.02 | 0.0496 |     880 B |        0.92 |
| ErrorOr                    | False   | True  | 3,149.58 ns | 40.296 ns | 37.693 ns |  1.01 |    0.02 | 0.0610 |    1128 B |        1.18 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **85.17 ns** |  **0.637 ns** |  **0.565 ns** |  **1.00** |    **0.01** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   100.81 ns |  1.109 ns |  1.038 ns |  1.18 |    0.01 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   105.79 ns |  1.435 ns |  1.272 ns |  1.24 |    0.02 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **3,098.28 ns** | **39.727 ns** | **37.161 ns** |  **1.00** |    **0.02** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 3,114.88 ns | 35.549 ns | 33.252 ns |  1.01 |    0.02 | 0.0496 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 3,116.52 ns | 41.003 ns | 38.354 ns |  1.01 |    0.02 | 0.0610 |    1040 B |        1.15 |
