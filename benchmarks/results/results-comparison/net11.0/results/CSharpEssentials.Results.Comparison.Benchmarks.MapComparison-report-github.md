```

BenchmarkDotNet v0.14.0, CachyOS
AMD Ryzen 5 5600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.5322 ns** | **0.0065 ns** | **0.0054 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9523 ns | 0.0013 ns | 0.0011 ns |   3.67 |    0.04 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8146 ns | 0.0003 ns | 0.0002 ns |   1.53 |    0.01 |      - |         - |          NA |
| FluentResults              | False   | 90.3252 ns | 1.1422 ns | 1.0684 ns | 169.73 |    2.55 | 0.0205 |     344 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5778 ns** | **0.0046 ns** | **0.0041 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.8007 ns | 0.0060 ns | 0.0056 ns |   3.12 |    0.02 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5793 ns | 0.0046 ns | 0.0043 ns |   1.00 |    0.01 |      - |         - |          NA |
| FluentResults              | True    | 55.5509 ns | 0.3242 ns | 0.2874 ns |  96.15 |    0.81 | 0.0153 |     256 B |          NA |
