```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean       | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |-----------:|---------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |   **126.4 ns** |  **0.78 ns** |  **0.73 ns** |  **1.00** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |   115.5 ns |  0.65 ns |  0.60 ns |  0.91 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   150.3 ns |  0.81 ns |  0.67 ns |  1.19 | 0.0386 |     648 B |        1.21 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,658.3 ns** | **10.14 ns** |  **9.49 ns** |  **1.00** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,640.8 ns | 23.53 ns | 22.01 ns |  0.99 | 0.0496 |     880 B |        0.92 |
| ErrorOr                    | False   | True  | 1,920.0 ns | 16.35 ns | 15.29 ns |  1.16 | 0.0648 |    1127 B |        1.17 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |   **107.8 ns** |  **0.46 ns** |  **0.43 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |   120.3 ns |  0.91 ns |  0.85 ns |  1.12 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   130.9 ns |  0.62 ns |  0.55 ns |  1.21 | 0.0334 |     560 B |        1.17 |
|                            |         |       |            |          |          |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,672.1 ns** | **11.89 ns** | **11.12 ns** |  **1.00** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,684.8 ns |  9.05 ns |  8.46 ns |  1.01 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,923.3 ns | 12.14 ns | 10.76 ns |  1.15 | 0.0610 |    1037 B |        1.15 |
