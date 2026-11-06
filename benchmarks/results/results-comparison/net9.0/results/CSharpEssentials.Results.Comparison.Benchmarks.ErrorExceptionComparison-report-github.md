```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.19 (9.0.1926.36724), X64 RyuJIT AVX2


```
| Method                   | Thrown | Mean        | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |------- |------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**         | **False**  |    **97.36 ns** |  **0.240 ns** |  **0.225 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| CSharpEssentials_Failure | False  |    12.14 ns |  0.003 ns |  0.003 ns |  0.12 |      - |         - |        0.00 |
| ErrorOr                  | False  |    12.20 ns |  0.070 ns |  0.066 ns |  0.13 |      - |         - |        0.00 |
| FluentResults            | False  |    38.05 ns |  0.314 ns |  0.278 ns |  0.39 | 0.0095 |     160 B |        0.74 |
|                          |        |             |           |           |       |        |           |             |
| **CSharpEssentials**         | **True**   | **3,131.51 ns** | **32.457 ns** | **30.360 ns** | **1.000** | **0.2747** |    **4720 B** |        **1.00** |
| CSharpEssentials_Failure | True   |    12.58 ns |  0.004 ns |  0.004 ns | 0.004 |      - |         - |        0.00 |
| ErrorOr                  | True   |    16.18 ns |  0.003 ns |  0.002 ns | 0.005 |      - |         - |        0.00 |
| FluentResults            | True   |    38.32 ns |  0.209 ns |  0.196 ns | 0.012 | 0.0095 |     160 B |        0.03 |
