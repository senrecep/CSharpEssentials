```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials_Typed**     | **False**   |   **9.232 ns** | **0.0152 ns** | **0.0142 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | False   |   8.876 ns | 0.0165 ns | 0.0154 ns |  0.96 |    0.00 |      - |         - |          NA |
| OneOf                      | False   |  31.400 ns | 0.0436 ns | 0.0407 ns |  3.40 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   8.270 ns | 0.0044 ns | 0.0034 ns |  0.90 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.961 ns | 0.0153 ns | 0.0144 ns |  0.97 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 542.219 ns | 1.2990 ns | 1.2151 ns | 58.73 |    0.15 | 0.0658 |    1104 B |          NA |
|                            |         |            |           |           |       |         |        |           |             |
| **CSharpEssentials_Typed**     | **True**    |  **17.264 ns** | **0.0170 ns** | **0.0159 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Result    | True    |   6.856 ns | 0.0264 ns | 0.0247 ns |  0.40 |    0.00 |      - |         - |          NA |
| OneOf                      | True    |  35.519 ns | 0.1186 ns | 0.1051 ns |  2.06 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   5.676 ns | 0.0085 ns | 0.0075 ns |  0.33 |    0.00 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |  45.966 ns | 0.0999 ns | 0.0934 ns |  2.66 |    0.01 | 0.0014 |      24 B |          NA |
| FluentResults              | True    | 490.331 ns | 2.9570 ns | 2.7659 ns | 28.40 |    0.16 | 0.0658 |    1104 B |          NA |
