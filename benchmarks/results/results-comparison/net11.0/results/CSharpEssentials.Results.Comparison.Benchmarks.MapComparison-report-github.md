```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2
  DefaultJob : .NET 11.0.0 (11.0.26.42628), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean       | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |-----------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |  **0.5533 ns** | **0.0129 ns** | **0.0120 ns** |   **1.00** |    **0.03** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |  1.9872 ns | 0.0129 ns | 0.0121 ns |   3.59 |    0.08 |      - |         - |          NA |
| ErrorOr                    | False   |  0.8288 ns | 0.0077 ns | 0.0064 ns |   1.50 |    0.03 |      - |         - |          NA |
| FluentResults              | False   | 92.8358 ns | 0.7542 ns | 0.7055 ns | 167.87 |    3.75 | 0.0205 |     344 B |          NA |
|                            |         |            |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |  **0.5651 ns** | **0.0117 ns** | **0.0103 ns** |   **1.00** |    **0.02** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |  1.7713 ns | 0.0130 ns | 0.0122 ns |   3.14 |    0.06 |      - |         - |          NA |
| ErrorOr                    | True    |  0.5745 ns | 0.0056 ns | 0.0053 ns |   1.02 |    0.02 |      - |         - |          NA |
| FluentResults              | True    | 60.6824 ns | 0.6607 ns | 0.6180 ns | 107.42 |    2.14 | 0.0153 |     256 B |          NA |
