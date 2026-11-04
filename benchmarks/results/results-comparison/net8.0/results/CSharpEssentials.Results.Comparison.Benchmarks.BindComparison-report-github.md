```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **1.323 ns** | **0.0015 ns** | **0.0014 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.904 ns | 0.0026 ns | 0.0025 ns |   6.73 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   2.042 ns | 0.0007 ns | 0.0006 ns |   1.54 |    0.00 |      - |         - |          NA |
| FluentResults              | False   |  89.589 ns | 0.4947 ns | 0.4627 ns |  67.71 |    0.35 | 0.0129 |     216 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **1.305 ns** | **0.0552 ns** | **0.0516 ns** |   **1.00** |    **0.06** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   8.921 ns | 0.0032 ns | 0.0028 ns |   6.85 |    0.27 |      - |         - |          NA |
| ErrorOr                    | True    |   1.253 ns | 0.0231 ns | 0.0216 ns |   0.96 |    0.04 |      - |         - |          NA |
| FluentResults              | True    | 150.889 ns | 0.9769 ns | 0.9138 ns | 115.82 |    4.69 | 0.0210 |     352 B |          NA |
