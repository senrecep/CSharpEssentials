```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.30 (8.0.3026.36720), X64 RyuJIT AVX2


```
| Method                   | Thrown | Mean        | Error     | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |------- |------------:|----------:|---------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**         | **False**  |   **124.24 ns** |  **0.257 ns** | **0.240 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| CSharpEssentials_Failure | False  |    17.94 ns |  0.016 ns | 0.014 ns |  0.14 |      - |         - |        0.00 |
| ErrorOr                  | False  |    13.32 ns |  0.005 ns | 0.005 ns |  0.11 |      - |         - |        0.00 |
| FluentResults            | False  |    37.22 ns |  0.247 ns | 0.192 ns |  0.30 | 0.0095 |     160 B |        0.74 |
|                          |        |             |           |          |       |        |           |             |
| **CSharpEssentials**         | **True**   | **3,479.46 ns** | **11.698 ns** | **9.769 ns** | **1.000** | **0.2747** |    **4720 B** |        **1.00** |
| CSharpEssentials_Failure | True   |    18.55 ns |  0.068 ns | 0.060 ns | 0.005 |      - |         - |        0.00 |
| ErrorOr                  | True   |    13.31 ns |  0.006 ns | 0.006 ns | 0.004 |      - |         - |        0.00 |
| FluentResults            | True   |    36.76 ns |  0.328 ns | 0.307 ns | 0.011 | 0.0095 |     160 B |        0.03 |
