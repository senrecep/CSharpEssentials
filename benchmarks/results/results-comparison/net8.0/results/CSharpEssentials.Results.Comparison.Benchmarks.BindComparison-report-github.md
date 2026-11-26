```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.33 (8.0.3326.52219), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **2.386 ns** | **0.0144 ns** | **0.0135 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | False   |   7.489 ns | 0.0057 ns | 0.0053 ns |   3.14 |    0.02 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.909 ns | 0.0051 ns | 0.0045 ns |   3.73 |    0.02 |      - |         - |          NA |
| ErrorOr                    | False   |   2.354 ns | 0.0034 ns | 0.0031 ns |   0.99 |    0.01 |      - |         - |          NA |
| FluentResults              | False   |  81.574 ns | 0.5217 ns | 0.4880 ns |  34.18 |    0.27 | 0.0129 |     216 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **1.379 ns** | **0.0033 ns** | **0.0031 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | True    |   6.032 ns | 0.0105 ns | 0.0098 ns |   4.38 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |   8.928 ns | 0.0037 ns | 0.0034 ns |   6.48 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   1.470 ns | 0.0466 ns | 0.0436 ns |   1.07 |    0.03 |      - |         - |          NA |
| FluentResults              | True    | 151.305 ns | 0.5827 ns | 0.4865 ns | 109.75 |    0.41 | 0.0210 |     352 B |          NA |
