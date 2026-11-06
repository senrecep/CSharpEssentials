```

BenchmarkDotNet v0.14.0, CachyOS
x64 desktop CPU, 1 CPU, 12 logical and 6 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.11 (10.0.1126.37416), X64 RyuJIT AVX2


```
| Method                   | Thrown | Mean         | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------------------------- |------- |-------------:|----------:|----------:|------:|-------:|----------:|------------:|
| **CSharpEssentials**         | **False**  |    **66.286 ns** | **0.4251 ns** | **0.3976 ns** |  **1.00** | **0.0129** |     **216 B** |        **1.00** |
| CSharpEssentials_Failure | False  |     5.215 ns | 0.0019 ns | 0.0015 ns |  0.08 |      - |         - |        0.00 |
| ErrorOr                  | False  |     5.229 ns | 0.0014 ns | 0.0012 ns |  0.08 |      - |         - |        0.00 |
| FluentResults            | False  |    26.358 ns | 0.3699 ns | 0.3089 ns |  0.40 | 0.0095 |     160 B |        0.74 |
|                          |        |              |           |           |       |        |           |             |
| **CSharpEssentials**         | **True**   | **2,719.927 ns** | **8.6508 ns** | **7.2238 ns** | **1.000** | **0.2747** |    **4720 B** |        **1.00** |
| CSharpEssentials_Failure | True   |     5.215 ns | 0.0018 ns | 0.0016 ns | 0.002 |      - |         - |        0.00 |
| ErrorOr                  | True   |     5.215 ns | 0.0021 ns | 0.0019 ns | 0.002 |      - |         - |        0.00 |
| FluentResults            | True   |    26.918 ns | 0.3055 ns | 0.2857 ns | 0.010 | 0.0095 |     160 B |        0.03 |
