```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                     | Succeed | Mean        | Error     | StdDev    | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|--------------------------- |-------- |------------:|----------:|----------:|-------:|--------:|-------:|----------:|------------:|
| **CSharpEssentials**           | **False**   |   **2.0419 ns** | **0.0014 ns** | **0.0011 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | False   |   8.9207 ns | 0.0216 ns | 0.0181 ns |   4.37 |    0.01 |      - |         - |          NA |
| ErrorOr                    | False   |   2.0340 ns | 0.0027 ns | 0.0022 ns |   1.00 |    0.00 |      - |         - |          NA |
| FluentResults              | False   | 163.0962 ns | 0.7002 ns | 0.6207 ns |  79.88 |    0.30 | 0.0219 |     368 B |          NA |
|                            |         |             |           |           |        |         |        |           |             |
| **CSharpEssentials**           | **True**    |   **0.8658 ns** | **0.0006 ns** | **0.0005 ns** |   **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
| CSharpFunctionalExtensions | True    |   8.9349 ns | 0.0034 ns | 0.0027 ns |  10.32 |    0.01 |      - |         - |          NA |
| ErrorOr                    | True    |   0.7119 ns | 0.0007 ns | 0.0005 ns |   0.82 |    0.00 |      - |         - |          NA |
| FluentResults              | True    | 139.7369 ns | 0.8252 ns | 0.7719 ns | 161.40 |    0.87 | 0.0172 |     288 B |          NA |
