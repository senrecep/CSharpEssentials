```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **118.2 ns** |  **1.01 ns** |  **0.94 ns** |  **1.00** |    **0.01** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   112.7 ns |  0.88 ns |  0.83 ns |  0.95 |    0.01 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   145.5 ns |  2.36 ns |  2.21 ns |  1.23 |    0.02 | 0.0386 |     648 B |        1.21 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,611.2 ns** |  **9.64 ns** |  **9.02 ns** |  **1.00** |    **0.01** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,598.8 ns |  9.93 ns |  9.29 ns |  0.99 |    0.01 | 0.0515 |     880 B |        0.92 |
| ErrorOr                    | False   | True  | 1,906.4 ns |  7.41 ns |  6.57 ns |  1.18 |    0.01 | 0.0668 |    1127 B |        1.17 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |   **102.2 ns** |  **0.70 ns** |  **0.66 ns** |  **1.00** |    **0.01** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   115.3 ns |  0.50 ns |  0.44 ns |  1.13 |    0.01 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   124.0 ns |  1.24 ns |  1.16 ns |  1.21 |    0.01 | 0.0334 |     560 B |        1.17 |
|                            |         |       |            |          |          |       |         |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,653.0 ns** |  **9.65 ns** |  **9.02 ns** |  **1.00** |    **0.01** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,644.0 ns | 10.41 ns |  9.74 ns |  0.99 |    0.01 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,901.9 ns | 12.10 ns | 11.32 ns |  1.15 |    0.01 | 0.0610 |    1037 B |        1.15 |
