```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100
  [Host]     : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.22 (9.0.2226.52218), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **1.6394 ns** | **0.0216 ns** | **0.0202 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | False   |   5.5150 ns | 0.0037 ns | 0.0032 ns |   3.36 |    0.04 |      - |         - |          NA |
| CSharpFunctionalExtensions | False   |   8.9922 ns | 0.0278 ns | 0.0260 ns |   5.49 |    0.07 |      - |         - |          NA |
| ErrorOr                    | False   |   0.8654 ns | 0.0038 ns | 0.0036 ns |   0.53 |    0.01 |      - |         - |          NA |
| FluentResults              | False   |  71.9494 ns | 0.4095 ns | 0.3197 ns |  43.89 |    0.56 | 0.0124 |     208 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **1.1069 ns** | **0.0013 ns** | **0.0011 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpEssentials_Typed     | True    |   5.7298 ns | 0.0058 ns | 0.0055 ns |   5.18 |    0.01 |      - |         - |          NA |
| CSharpFunctionalExtensions | True    |   8.9661 ns | 0.0200 ns | 0.0187 ns |   8.10 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |   0.8994 ns | 0.0064 ns | 0.0060 ns |   0.81 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 119.4795 ns | 0.9027 ns | 0.8444 ns | 107.94 |    0.75 | 0.0191 |     320 B |          NA |
