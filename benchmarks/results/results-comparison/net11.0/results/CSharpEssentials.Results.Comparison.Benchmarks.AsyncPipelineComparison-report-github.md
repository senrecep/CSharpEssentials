```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.3.26522.148
  [Host]     : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.52348), X64 RyuJIT AVX2


```
| Method                     | Succeed | Yield | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   | **False** |    **96.42 ns** |  **0.875 ns** |  **0.818 ns** |  **1.00** | **0.0319** |     **536 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | False |    98.24 ns |  0.771 ns |  0.721 ns |  1.02 | 0.0286 |     480 B |        0.90 |
| ErrorOr                    | False   | False |   113.05 ns |  0.494 ns |  0.386 ns |  1.17 | 0.0386 |     648 B |        1.21 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **False**   | **True**  | **1,179.38 ns** | **10.244 ns** |  **9.582 ns** |  **1.00** | **0.0572** |     **960 B** |        **1.00** |
| CSharpFunctionalExtensions | False   | True  | 1,170.73 ns | 10.499 ns |  9.307 ns |  0.99 | 0.0515 |     879 B |        0.92 |
| ErrorOr                    | False   | True  | 1,332.85 ns | 11.999 ns | 11.224 ns |  1.13 | 0.0668 |    1127 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **False** |    **83.03 ns** |  **0.713 ns** |  **0.667 ns** |  **1.00** | **0.0286** |     **480 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | False |    93.77 ns |  0.470 ns |  0.440 ns |  1.13 | 0.0286 |     480 B |        1.00 |
| ErrorOr                    | True    | False |   109.93 ns |  0.926 ns |  0.821 ns |  1.32 | 0.0334 |     560 B |        1.17 |
|                            |         |       |             |           |           |       |        |           |             |
| **CSharpEssentials**           | **True**    | **True**  | **1,179.85 ns** | **12.443 ns** | **11.031 ns** |  **1.00** | **0.0534** |     **904 B** |        **1.00** |
| CSharpFunctionalExtensions | True    | True  | 1,201.19 ns | 12.972 ns | 12.134 ns |  1.02 | 0.0515 |     880 B |        0.97 |
| ErrorOr                    | True    | True  | 1,338.94 ns |  9.822 ns |  9.187 ns |  1.13 | 0.0610 |    1037 B |        1.15 |
