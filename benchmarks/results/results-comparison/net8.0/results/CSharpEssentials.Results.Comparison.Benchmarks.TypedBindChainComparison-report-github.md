```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **8.836 ns** | **0.0217 ns** | **0.0192 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   8.860 ns | 0.0073 ns | 0.0068 ns |  1.00 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  31.114 ns | 0.0925 ns | 0.0865 ns |  3.52 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   8.275 ns | 0.0095 ns | 0.0074 ns |  0.94 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.945 ns | 0.0075 ns | 0.0066 ns |  1.01 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 490.270 ns | 2.7577 ns | 2.5795 ns | 55.48 |    0.31 | 0.0658 |    1104 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |  **16.608 ns** | **0.0305 ns** | **0.0285 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.823 ns | 0.0037 ns | 0.0031 ns |  0.41 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.603 ns | 0.0867 ns | 0.0769 ns |  1.90 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   5.598 ns | 0.0023 ns | 0.0019 ns |  0.34 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  42.768 ns | 0.1490 ns | 0.1394 ns |  2.58 |    0.01 | 0.0014 |      24 B |          NA |
| FluentResults              | True    | 501.739 ns | 2.6366 ns | 2.4663 ns | 30.21 |    0.15 | 0.0658 |    1104 B |          NA |
