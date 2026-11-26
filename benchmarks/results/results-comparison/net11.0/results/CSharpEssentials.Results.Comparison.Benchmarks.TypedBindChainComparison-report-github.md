```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.53018), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **8.892 ns** | **0.0214 ns** | **0.0200 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   4.557 ns | 0.0065 ns | 0.0058 ns |  0.51 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  30.791 ns | 0.1565 ns | 0.1464 ns |  3.46 |    0.02 |      - |         - |          NA |
| ErrorOr                    | False   |   3.364 ns | 0.0036 ns | 0.0034 ns |  0.38 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   7.375 ns | 0.0142 ns | 0.0133 ns |  0.83 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 300.742 ns | 1.4526 ns | 1.3588 ns | 33.82 |    0.17 | 0.0596 |    1000 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |   **8.285 ns** | **0.0103 ns** | **0.0091 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   5.666 ns | 0.0044 ns | 0.0036 ns |  0.68 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  31.766 ns | 0.0312 ns | 0.0292 ns |  3.83 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   4.812 ns | 0.0166 ns | 0.0147 ns |  0.58 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |   7.689 ns | 0.0158 ns | 0.0132 ns |  0.93 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 225.564 ns | 2.1911 ns | 2.0496 ns | 27.23 |    0.24 | 0.0591 |     992 B |          NA |
